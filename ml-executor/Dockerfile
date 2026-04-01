FROM python:3.12-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

ENV ML_EXECUTOR_DATASET_ROOT=/app/shared/datasets
ENV ML_EXECUTOR_ARTIFACT_ROOT=/app/shared/ml-artifacts
ENV ML_EXECUTOR_SHARED_SECRET=AstraLab_ML_Executor_SharedSecret_2026
ENV ML_EXECUTOR_HTTP_TIMEOUT_SECONDS=30

EXPOSE 8010

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8010"]
