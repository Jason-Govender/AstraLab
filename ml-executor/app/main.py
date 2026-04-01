from __future__ import annotations

from fastapi import Depends, FastAPI, Header, HTTPException, status

from .job_executor import JobExecutor
from .job_registry import JobRegistry
from .schemas import JobAcceptedResponse, JobRequest
from .settings import get_settings


app = FastAPI(title="AstraLab ML Executor", version="1.0.0")

SETTINGS = get_settings()
REGISTRY = JobRegistry()
EXECUTOR = JobExecutor(SETTINGS, REGISTRY)


def require_shared_secret(
    shared_secret: str | None = Header(default=None, alias="X-AstraLab-ML-Secret"),
) -> None:
    if shared_secret != SETTINGS.shared_secret:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="The ML executor shared secret is invalid.",
        )


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/jobs", status_code=status.HTTP_202_ACCEPTED, response_model=JobAcceptedResponse)
async def create_job(
    job: JobRequest,
    _: None = Depends(require_shared_secret),
) -> JobAcceptedResponse:
    return await EXECUTOR.enqueue(job)
