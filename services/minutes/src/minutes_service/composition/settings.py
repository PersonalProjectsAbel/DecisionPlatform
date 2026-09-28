from typing import Literal

from pydantic import SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    ai_provider: Literal["gemini"] = "gemini"
    gemini_api_key: SecretStr | None = None
    gemini_model: str = "gemini-3.8-flash"
    kurrent_connection_string: str = "kurrentdb://127.0.0.1:2113?tls=false"
    minutes_subscription_group: str = "minutes-draft-generation-v1"
    decision_planner_api_url: str = "http://127.0.0.1:5189"
    minutes_service_api_key: SecretStr | None = None
