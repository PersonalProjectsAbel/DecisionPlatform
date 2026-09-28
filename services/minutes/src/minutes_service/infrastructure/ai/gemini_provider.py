from google import genai
from google.genai import types
from pydantic import SecretStr


class GeminiProvider:
    """Gemini implementation of the application AIProvider port."""

    def __init__(self, api_key: SecretStr | None, model: str) -> None:
        self._api_key = api_key
        self._model = model
        self._client: genai.Client | None = None

    async def generate(self, system_prompt: str, user_prompt: str) -> str:
        if self._api_key is None or not self._api_key.get_secret_value().strip():
            raise RuntimeError("GEMINI_API_KEY must be configured to use GeminiProvider")

        if self._client is None:
            self._client = genai.Client(api_key=self._api_key.get_secret_value())

        response = await self._client.aio.models.generate_content(
            model=self._model,
            contents=user_prompt,
            config=types.GenerateContentConfig(system_instruction=system_prompt),
        )
        text = response.text
        if text is None or not text.strip():
            raise RuntimeError("Gemini returned an empty text response")
        return text

    async def close(self) -> None:
        if self._client is not None:
            await self._client.aio.aclose()
            self._client = None
