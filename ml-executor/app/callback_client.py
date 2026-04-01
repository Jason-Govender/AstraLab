from __future__ import annotations

import asyncio
import httpx

from .schemas import CompletionCallbackPayload, FailureCallbackPayload
from .settings import ExecutorSettings


SHARED_SECRET_HEADER_NAME = "X-AstraLab-ML-Secret"
CALLBACK_MAX_ATTEMPTS = 8
CALLBACK_MAX_DELAY_SECONDS = 30


async def post_completion_callback(
    settings: ExecutorSettings,
    callback_url: str,
    payload: CompletionCallbackPayload,
) -> None:
    await post_callback(settings, callback_url, payload.model_dump(by_alias=True))


async def post_failure_callback(
    settings: ExecutorSettings,
    callback_url: str,
    payload: FailureCallbackPayload,
) -> None:
    await post_callback(settings, callback_url, payload.model_dump(by_alias=True))


async def post_callback(
    settings: ExecutorSettings,
    callback_url: str,
    payload: dict,
) -> None:
    headers = {SHARED_SECRET_HEADER_NAME: settings.shared_secret}

    last_exception: Exception | None = None

    for attempt in range(1, CALLBACK_MAX_ATTEMPTS + 1):
        try:
            async with httpx.AsyncClient(timeout=settings.http_timeout_seconds) as client:
                response = await client.post(callback_url, json=payload, headers=headers)
                response.raise_for_status()
                return
        except Exception as exception:
            last_exception = exception
            if attempt == CALLBACK_MAX_ATTEMPTS:
                break

            await asyncio.sleep(min(2 ** (attempt - 1), CALLBACK_MAX_DELAY_SECONDS))

    if last_exception is not None:
        raise last_exception
