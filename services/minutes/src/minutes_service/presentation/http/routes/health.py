import logging

from fastapi import APIRouter, HTTPException, Request

router = APIRouter()
logger = logging.getLogger(__name__)


@router.get("/health", tags=["health"])
async def health() -> dict[str, str]:
    return {"status": "ok"}


@router.get("/health/ai-provider", tags=["health"])
async def ai_provider_health(request: Request) -> dict[str, str]:
    """Make one small generation request to check the configured AI provider."""
    settings = request.app.state.settings
    provider = request.app.state.container.ai_provider()

    try:
        await provider.generate(
            system_prompt="Reply with only the word OK.",
            user_prompt="This is a connectivity check. Reply with OK.",
        )
    except RuntimeError as error:
        raise HTTPException(status_code=503, detail=str(error)) from error
    except Exception as error:
        logger.warning(
            "AI provider health check failed (%s).",
            type(error).__name__,
        )
        raise HTTPException(
            status_code=502,
            detail="The AI provider request failed. Check the API key, model, and service logs.",
        ) from error

    return {
        "status": "healthy",
        "provider": settings.ai_provider,
        "model": settings.gemini_model,
    }
