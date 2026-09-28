import json
from datetime import datetime
from uuid import UUID

from kurrentdbclient import AsyncKurrentDBClient

from minutes_service.application.features.generate_minutes_draft.models import (
    DiscussionEntry,
    MeetingContext,
    Proposal,
    Topic,
)


class KurrentMeetingContextReader:
    """Rebuilds the prompt input from a Meeting event stream."""

    def __init__(self, client: AsyncKurrentDBClient) -> None:
        self._client = client

    async def read(self, meeting_id: str) -> MeetingContext:
        stream_name = f"meeting-{UUID(meeting_id).hex}"
        title: str | None = None
        description: str | None = None
        participants: list[str] = []
        topics: dict[str, dict[str, object]] = {}

        async for recorded in await self._client.read_stream(stream_name):
            event = json.loads(bytes(recorded.data))
            event_type = recorded.type

            if event_type == "meeting.created.v1":
                title = event["title"]
                description = event.get("description")
            elif event_type == "meeting.participant-added.v1":
                participants.append(event["name"])
            elif event_type == "meeting.topic-created.v1":
                topics[event["topicId"]] = {
                    "title": event["title"],
                    "proposals": [],
                    "discussion_entries": [],
                }
            elif event_type == "meeting.proposal-created.v1":
                topic = topics.get(event["topicId"])
                if topic is not None:
                    proposals = topic["proposals"]
                    assert isinstance(proposals, list)
                    proposals.append(
                        Proposal(
                            proposal_id=event["proposalId"],
                            title=event["title"],
                            description=event.get("description"),
                        )
                    )
            elif event_type == "meeting.discussion-entry-added.v1":
                topic = topics.get(event["topicId"])
                if topic is not None:
                    entries = topic["discussion_entries"]
                    assert isinstance(entries, list)
                    proposal_id = event.get("proposalId")
                    proposals = topic["proposals"]
                    assert isinstance(proposals, list)
                    proposal = next(
                        (item for item in proposals if item.proposal_id == proposal_id),
                        None,
                    )
                    entries.append(
                        DiscussionEntry(
                            author_name=event["authorName"],
                            content=event["content"],
                            created_at=_parse_datetime(event["createdAt"]),
                            proposal_title=proposal.title if proposal else None,
                        )
                    )

        if title is None:
            raise ValueError(f"Meeting stream {stream_name} has no meeting-created event")

        if not topics:
            topics[meeting_id] = {
                "title": "General",
                "proposals": [],
                "discussion_entries": [],
            }

        return MeetingContext(
            meeting_id=meeting_id,
            title=title,
            description=description,
            participants=tuple(participants),
            topics=tuple(
                Topic(
                    title=str(data["title"]),
                    proposals=tuple(data["proposals"]),
                    discussion_entries=tuple(data["discussion_entries"]),
                )
                for data in topics.values()
            ),
        )


def _parse_datetime(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))
