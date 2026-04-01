from __future__ import annotations

import joblib

from .settings import ExecutorSettings


def save_artifact(
    settings: ExecutorSettings,
    tenant_id: int,
    experiment_id: int,
    model_bundle: object,
) -> tuple[str, str]:
    artifact_key = f"tenants/{tenant_id}/ml/experiments/{experiment_id}/model.joblib"
    artifact_path = (settings.artifact_root / artifact_key).resolve()
    artifact_path.parent.mkdir(parents=True, exist_ok=True)
    joblib.dump(model_bundle, artifact_path)
    return "local-filesystem", artifact_key
