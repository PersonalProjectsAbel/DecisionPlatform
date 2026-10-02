from contextlib import asynccontextmanager
from collections.abc import AsyncIterator

from fastapi import FastAPI

from minutes_service.composition.container import build_container
from minutes_service.composition.settings import Settings
from minutes_service.presentation.http.routes.health import router as health_router


def create_app(settings: Settings | None = None) -> FastAPI:
    active_settings = settings or Settings()
    container = build_container(active_settings)

    @asynccontextmanager
    async def lifespan(_: FastAPI) -> AsyncIterator[None]:
        yield
        if active_settings.ai_provider == "gemini":
            await container.gemini_provider().close()

    app = FastAPI(title="DecisionPlanner Minutes Service", lifespan=lifespan)
    app.state.container = container
    app.state.settings = active_settings
    app.include_router(health_router)
    return app
