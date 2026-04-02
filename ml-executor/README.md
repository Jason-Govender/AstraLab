# AstraLab ML Executor

## Overview

The AstraLab ML Executor is the Python service responsible for running machine learning jobs on behalf of the backend. It accepts authenticated job requests, executes training or evaluation work, and returns results that the main platform can persist and display in the ML workspace and analytics flows.

## Stack

- FastAPI
- pandas
- NumPy
- scikit-learn
- joblib
- httpx

## Entrypoint

The local application entrypoint is:

```text
app.main:app
```

## Endpoints

- `GET /health`
  - Simple health check for local development and service readiness checks.
- `POST /jobs`
  - Accepts ML job requests from the backend and requires the shared secret header used by the main application.

## Environment Variables

Required runtime configuration:

- `ML_EXECUTOR_SHARED_SECRET`
  - Shared secret expected from the backend in the `X-AstraLab-ML-Secret` header.
- `ML_EXECUTOR_HTTP_TIMEOUT_SECONDS`
  - Timeout used by the executor for outbound HTTP operations.

## Local Setup

Create and activate a virtual environment:

```bash
python -m venv .venv
```

Install dependencies:

```bash
pip install -r requirements.txt
```

Run the service locally:

```bash
uvicorn app.main:app --reload --port 8010
```

The backend is configured to call the executor over HTTP, so keep this service running while testing ML experiment flows locally.

## Integration Boundary

The executor is a focused worker service, not a full product surface. The ASP.NET backend is responsible for user authentication, experiment orchestration, persistence, and UI-facing APIs. The executor is responsible for accepting authenticated ML jobs, performing the requested computation, and returning results in the format expected by the backend.

## Related Documentation

- [Root README](../README.md)
- [Backend README](../aspnet-core/README.md)
- [Frontend README](../frontend/README.md)
