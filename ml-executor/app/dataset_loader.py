from __future__ import annotations

import json
from pathlib import Path

import pandas as pd


def load_dataset(dataset_root: Path, dataset_format: str, storage_key: str) -> pd.DataFrame:
    dataset_path = resolve_storage_path(dataset_root, storage_key)
    if dataset_format == "csv":
        return pd.read_csv(dataset_path)

    if dataset_format == "json":
        return load_tabular_json(dataset_path)

    raise ValueError(f"Unsupported dataset format: {dataset_format}")


def resolve_storage_path(storage_root: Path, storage_key: str) -> Path:
    candidate = (storage_root / storage_key).resolve()
    root = storage_root.resolve()

    try:
        candidate.relative_to(root)
    except ValueError as exception:
        raise ValueError("The dataset storage key resolves outside of the configured dataset root.") from exception

    if not candidate.exists():
        raise FileNotFoundError(f"The dataset file was not found at '{candidate}'.")

    return candidate


def load_tabular_json(dataset_path: Path) -> pd.DataFrame:
    with dataset_path.open("r", encoding="utf-8") as dataset_file:
        payload = json.load(dataset_file)

    if isinstance(payload, list):
        return pd.DataFrame(payload)

    if isinstance(payload, dict):
        if isinstance(payload.get("rows"), list):
            return pd.DataFrame(payload["rows"])

        if isinstance(payload.get("items"), list):
            return pd.DataFrame(payload["items"])

    raise ValueError("JSON datasets must contain a top-level array, rows array, or items array.")
