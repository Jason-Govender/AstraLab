from __future__ import annotations

import asyncio
from datetime import timezone

from .artifact_service import upload_artifact
from .callback_client import post_completion_callback, post_failure_callback
from .job_registry import JobRegistry
from .schemas import CompletionCallbackPayload, FailureCallbackPayload, JobAcceptedResponse, JobRequest
from .settings import ExecutorSettings
from .training_service import train_job


class JobExecutor:
    def __init__(self, settings: ExecutorSettings, registry: JobRegistry) -> None:
        self._settings = settings
        self._registry = registry

    async def enqueue(self, job: JobRequest) -> JobAcceptedResponse:
        record, is_new = await self._registry.accept(job.experiment_id)
        if is_new:
            asyncio.create_task(self._run(job))

        return JobAcceptedResponse(experimentId=job.experiment_id, status=record.status)

    async def _run(self, job: JobRequest) -> None:
        started_at_utc = None

        try:
            await self._registry.mark_running(job.experiment_id)
            result = train_job(job, self._settings)
            started_at_utc = result.started_at_utc
            upload_artifact(
                self._settings,
                job.artifact_upload_url,
                result.artifact_bundle,
            )

            await post_completion_callback(
                self._settings,
                job.completed_callback_url,
                CompletionCallbackPayload(
                    experimentId=job.experiment_id,
                    startedAtUtc=result.started_at_utc.astimezone(timezone.utc).isoformat(),
                    completedAtUtc=result.completed_at_utc.astimezone(timezone.utc).isoformat(),
                    modelType=result.model_type,
                    artifactStorageProvider=job.artifact_storage_provider,
                    artifactStorageKey=job.artifact_storage_key,
                    performanceSummaryJson=result.performance_summary_json,
                    warningsJson=result.warnings_json,
                    metrics=result.metrics,
                    featureImportances=result.feature_importances,
                ),
            )

            await self._registry.mark_completed(job.experiment_id)
        except Exception as exception:
            failure_callback_exception = None

            try:
                await post_failure_callback(
                    self._settings,
                    job.failed_callback_url,
                    FailureCallbackPayload(
                        experimentId=job.experiment_id,
                        startedAtUtc=started_at_utc.astimezone(timezone.utc).isoformat() if started_at_utc else None,
                        completedAtUtc=None,
                        failureMessage=str(exception),
                        warningsJson=None,
                    ),
                )
            except Exception as callback_exception:
                failure_callback_exception = callback_exception

            await self._registry.mark_failed(job.experiment_id)

            if failure_callback_exception is not None:
                raise failure_callback_exception from exception
