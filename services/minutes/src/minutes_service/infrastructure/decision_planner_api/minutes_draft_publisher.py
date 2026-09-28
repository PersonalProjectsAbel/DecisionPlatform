import httpx
from pydantic import SecretStr

from minutes_service.application.features.generate_minutes_draft.models import MinutesDraft


class DecisionPlannerMinutesDraftPublisher:
    """Sends a completed draft to the .NET API, which owns event appends."""

    def __init__(self, base_url: str, api_key: SecretStr | None) -> None:
        if api_key is None or not api_key.get_secret_value().strip():
            raise RuntimeError("MINUTES_SERVICE_API_KEY must be configured for the worker")
        self._api_key = api_key.get_secret_value()
        self._base_url = base_url.rstrip("/")
        self._client = httpx.AsyncClient(timeout=httpx.Timeout(30.0))

    async def publish(self, draft: MinutesDraft) -> None:
        response = await self._client.post(
            f"{self._base_url}/api/internal/meetings/{draft.meeting_id}/minutes-draft",
            headers={"X-Minutes-Service-Key": self._api_key},
            json={
                "completedEventId": draft.completed_event_id,
                "content": draft.content,
                "generatedAt": draft.generated_at.isoformat(),
            },
        )
        response.raise_for_status()

    async def close(self) -> None:
        await self._client.aclose()
