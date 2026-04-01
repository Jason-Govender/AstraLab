from dataclasses import dataclass
from pathlib import Path
import os


@dataclass(frozen=True)
class ExecutorSettings:
    shared_secret: str
    dataset_root: Path
    artifact_root: Path
    http_timeout_seconds: float


def get_settings() -> ExecutorSettings:
    dataset_root = Path(os.getenv("ML_EXECUTOR_DATASET_ROOT", "/app/shared/datasets")).resolve()
    artifact_root = Path(os.getenv("ML_EXECUTOR_ARTIFACT_ROOT", "/app/shared/ml-artifacts")).resolve()
    artifact_root.mkdir(parents=True, exist_ok=True)

    return ExecutorSettings(
        shared_secret=os.getenv("ML_EXECUTOR_SHARED_SECRET", "AstraLab_ML_Executor_SharedSecret_2026"),
        dataset_root=dataset_root,
        artifact_root=artifact_root,
        http_timeout_seconds=float(os.getenv("ML_EXECUTOR_HTTP_TIMEOUT_SECONDS", "30")),
    )
