from __future__ import annotations

import io
import json

import httpx
import pandas as pd


def load_dataset(dataset_download_url: str, dataset_format: str, timeout_seconds: float) -> pd.DataFrame:
    response = httpx.get(dataset_download_url, timeout=timeout_seconds)
    response.raise_for_status()

    if dataset_format == "csv":
        return pd.read_csv(io.BytesIO(response.content))

    if dataset_format == "json":
        return load_tabular_json(response.text)

    raise ValueError(f"Unsupported dataset format: {dataset_format}")


def load_tabular_json(json_payload: str) -> pd.DataFrame:
    payload = json.loads(json_payload)

    if isinstance(payload, list):
        return pd.DataFrame(payload)

    if isinstance(payload, dict):
        if isinstance(payload.get("rows"), list):
            return pd.DataFrame(payload["rows"])

        if isinstance(payload.get("items"), list):
            return pd.DataFrame(payload["items"])

    raise ValueError("JSON datasets must contain a top-level array, rows array, or items array.")
