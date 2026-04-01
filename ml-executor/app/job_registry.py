from __future__ import annotations

import asyncio
from dataclasses import dataclass
from datetime import datetime, timezone


@dataclass
class JobRecord:
    status: str
    updated_at_utc: datetime


class JobRegistry:
    def __init__(self) -> None:
        self._jobs: dict[int, JobRecord] = {}
        self._lock = asyncio.Lock()

    async def accept(self, experiment_id: int) -> tuple[JobRecord, bool]:
        async with self._lock:
            existing = self._jobs.get(experiment_id)
            if existing is not None:
                return existing, False

            record = JobRecord(status="accepted", updated_at_utc=datetime.now(timezone.utc))
            self._jobs[experiment_id] = record
            return record, True

    async def mark_running(self, experiment_id: int) -> None:
        await self._set_status(experiment_id, "running")

    async def mark_completed(self, experiment_id: int) -> None:
        await self._set_status(experiment_id, "completed")

    async def mark_failed(self, experiment_id: int) -> None:
        await self._set_status(experiment_id, "failed")

    async def _set_status(self, experiment_id: int, status: str) -> None:
        async with self._lock:
            self._jobs[experiment_id] = JobRecord(status=status, updated_at_utc=datetime.now(timezone.utc))
