from dataclasses import dataclass
import os


@dataclass(frozen=True)
class ExecutorSettings:
    shared_secret: str
    http_timeout_seconds: float


def get_settings() -> ExecutorSettings:
    return ExecutorSettings(
        shared_secret=os.getenv("ML_EXECUTOR_SHARED_SECRET", "AstraLab_ML_Executor_SharedSecret_2026"),
        http_timeout_seconds=float(os.getenv("ML_EXECUTOR_HTTP_TIMEOUT_SECONDS", "30")),
    )
