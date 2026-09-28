import asyncio
import json
import logging
from uuid import UUID

from kurrentdbclient import AsyncKurrentDBClient

from minutes_service.application.features.generate_minutes_draft.handler import (
    GenerateMinutesDraftHandler,
)
from minutes_service.infrastructure.decision_planner_api.minutes_draft_publisher import (
    DecisionPlannerMinutesDraftPublisher,
)
from minutes_service.infrastructure.kurrentdb.meeting_context_reader import (
    KurrentMeetingContextReader,
)

logger = logging.getLogger(__name__)
MEETING_COMPLETED_EVENT = "meeting.completed.v1"


class MeetingMinutesWorker:
    def __init__(
        self,
        client: AsyncKurrentDBClient,
        group_name: str,
        context_reader: KurrentMeetingContextReader,
        generator: GenerateMinutesDraftHandler,
        publisher: DecisionPlannerMinutesDraftPublisher,
    ) -> None:
        self._client = client
        self._group_name = group_name
        self._context_reader = context_reader
        self._generator = generator
        self._publisher = publisher

    async def run(self) -> None:
        await self._ensure_subscription()
        while True:
            try:
                await self._consume()
            except asyncio.CancelledError:
                raise
            except Exception:
                logger.exception("Minutes subscription disconnected; reconnecting")
                await asyncio.sleep(3)

    async def _ensure_subscription(self) -> None:
        subscriptions = await self._client.list_subscriptions()
        if any(item.group_name == self._group_name for item in subscriptions):
            return
        await self._client.create_subscription_to_all(
            group_name=self._group_name,
            from_end=True,
            filter_include=[r"meeting\.completed\.v1"],
            message_timeout=300,
            max_retry_count=10,
            checkpoint_after=2,
        )
        logger.info("Created persistent subscription %s", self._group_name)

    async def _consume(self) -> None:
        async with await self._client.read_subscription_to_all(
            group_name=self._group_name,
        ) as subscription:
            async for event in subscription:
                try:
                    await self._process(event)
                    await subscription.ack(event)
                except asyncio.CancelledError:
                    raise
                except Exception as error:
                    logger.exception("Minutes generation failed for event %s", event.id)
                    await subscription.nack(event, action="retry")

    async def _process(self, event: object) -> None:
        data = json.loads(bytes(event.data))
        meeting_id = str(UUID(data["meetingId"]))
        expected_stream = f"meeting-{UUID(meeting_id).hex}"
        if event.stream_name != expected_stream:
            raise ValueError("Completion event meeting ID does not match its stream")
        context = await self._context_reader.read(meeting_id)
        draft = await self._generator.handle(context, str(event.id))
        await self._publisher.publish(draft)
        logger.info("Published minutes draft for meeting %s", meeting_id)
