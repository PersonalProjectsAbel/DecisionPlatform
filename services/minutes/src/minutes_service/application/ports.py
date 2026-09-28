from typing import Protocol

from minutes_service.application.features.generate_minutes_draft.models import (
    MeetingContext,
    MinutesDraft,
)


class AIProvider(Protocol):
    """Provider-neutral text generation capability used by application code."""

    async def generate(self, system_prompt: str, user_prompt: str) -> str:
        """Generate text from system and user prompts."""


class MeetingContextReader(Protocol):
    async def read(self, meeting_id: str) -> MeetingContext: ...


class MinutesDraftPublisher(Protocol):
    async def publish(self, draft: MinutesDraft) -> None: ...
