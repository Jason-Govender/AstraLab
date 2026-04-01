from __future__ import annotations

import io

import httpx
import joblib

from .settings import ExecutorSettings


def upload_artifact(
    settings: ExecutorSettings,
    artifact_upload_url: str,
    model_bundle: object,
) -> None:
    buffer = io.BytesIO()
    joblib.dump(model_bundle, buffer)
    buffer.seek(0)

    response = httpx.put(
        artifact_upload_url,
        content=buffer.getvalue(),
        headers={"Content-Type": "application/octet-stream"},
        timeout=settings.http_timeout_seconds,
    )
    response.raise_for_status()
