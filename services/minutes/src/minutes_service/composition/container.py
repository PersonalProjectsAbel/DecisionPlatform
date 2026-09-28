from dependency_injector import containers, providers
from kurrentdbclient import AsyncKurrentDBClient

from minutes_service.application.features.generate_minutes_draft.handler import (
    GenerateMinutesDraftHandler,
)
from minutes_service.composition.settings import Settings
from minutes_service.infrastructure.ai.gemini_provider import GeminiProvider
from minutes_service.infrastructure.decision_planner_api.minutes_draft_publisher import (
    DecisionPlannerMinutesDraftPublisher,
)
from minutes_service.infrastructure.kurrentdb.meeting_context_reader import (
    KurrentMeetingContextReader,
)
from minutes_service.infrastructure.minutes_worker import MeetingMinutesWorker


class Container(containers.DeclarativeContainer):
    """Composition root: maps application ports to infrastructure adapters."""

    settings = providers.Dependency(instance_of=Settings)

    gemini_provider = providers.Singleton(
        GeminiProvider,
        api_key=settings.provided.gemini_api_key,
        model=settings.provided.gemini_model,
    )
    ai_provider = providers.Selector(
        settings.provided.ai_provider,
        gemini=gemini_provider,
    )
    kurrent_client = providers.Singleton(
        AsyncKurrentDBClient,
        settings.provided.kurrent_connection_string,
    )
    meeting_context_reader = providers.Factory(
        KurrentMeetingContextReader,
        client=kurrent_client,
    )
    minutes_draft_publisher = providers.Singleton(
        DecisionPlannerMinutesDraftPublisher,
        base_url=settings.provided.decision_planner_api_url,
        api_key=settings.provided.minutes_service_api_key,
    )
    generate_minutes_draft = providers.Factory(
        GenerateMinutesDraftHandler,
        ai_provider=ai_provider,
    )
    minutes_worker = providers.Factory(
        MeetingMinutesWorker,
        client=kurrent_client,
        group_name=settings.provided.minutes_subscription_group,
        context_reader=meeting_context_reader,
        generator=generate_minutes_draft,
        publisher=minutes_draft_publisher,
    )


def build_container(settings: Settings) -> Container:
    container = Container()
    container.settings.override(providers.Object(settings))
    return container
