import json
import tempfile
import unittest
from pathlib import Path

from app.dataset_loader import load_dataset


class DatasetLoaderTests(unittest.TestCase):
    def test_should_load_json_rows_payload(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            dataset_root = Path(temporary_directory)
            dataset_path = dataset_root / "rows.json"
            dataset_path.write_text(json.dumps({"rows": [{"age": 30}, {"age": 31}]}), encoding="utf-8")

            dataframe = load_dataset(dataset_root, "json", "rows.json")

            self.assertEqual(list(dataframe.columns), ["age"])
            self.assertEqual(len(dataframe.index), 2)


if __name__ == "__main__":
    unittest.main()
