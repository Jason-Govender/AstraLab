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
                datasetStorageProvider="local-filesystem",
                datasetStorageKey="tenants/1/data.csv",
                taskType="classification",
                algorithmKey="logistic_regression",
                trainingConfigurationJson="{}",
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
