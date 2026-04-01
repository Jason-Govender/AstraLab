import unittest

from app.schemas import JobRequest


class JobRequestTests(unittest.TestCase):
    def test_should_require_target_for_classification(self) -> None:
        with self.assertRaises(ValueError):
            JobRequest(
                experimentId=1,
                tenantId=1,
                datasetVersionId=2,
                datasetFormat="csv",
                datasetDownloadUrl="http://localhost/datasets/1",
                taskType="classification",
                algorithmKey="logistic_regression",
                trainingConfigurationJson="{}",
                artifactUploadUrl="http://localhost/artifacts/1",
                artifactStorageProvider="s3-compatible",
                artifactStorageKey="tenants/1/ml/experiments/1/model.joblib",
                featureColumns=[
                    {
                        "datasetColumnId": 10,
                        "name": "age",
                        "dataType": "integer",
                        "ordinal": 1,
                    }
                ],
                completedCallbackUrl="http://localhost/completed",
                failedCallbackUrl="http://localhost/failed",
            )


if __name__ == "__main__":
    unittest.main()
