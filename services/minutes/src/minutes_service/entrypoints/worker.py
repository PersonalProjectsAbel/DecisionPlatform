import asyncio
import logging

from minutes_service.composition.container import build_container
from minutes_service.composition.settings import Settings


async def main() -> None:
    logging.basicConfig(level=logging.INFO)
    settings = Settings()
    if settings.ai_provider != "gemini":
        raise RuntimeError(f"Unsupported AI provider: {settings.ai_provider}")

    container = build_container(settings)
    client = container.kurrent_client()
    publisher = container.minutes_draft_publisher()
    await client.connect()
    try:
        await container.minutes_worker().run()
    finally:
        await publisher.close()
        await container.gemini_provider().close()
        await client.close()


if __name__ == "__main__":
    asyncio.run(main())
