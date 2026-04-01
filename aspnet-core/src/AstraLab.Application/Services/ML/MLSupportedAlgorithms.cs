using System;
using System.Collections.Generic;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Defines the supported baseline algorithms for each ML task type.
    /// </summary>
    public static class MLSupportedAlgorithms
    {
        private static readonly Dictionary<MLTaskType, HashSet<string>> AlgorithmsByTask =
            new Dictionary<MLTaskType, HashSet<string>>
            {
                {
                    MLTaskType.Classification,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "logistic_regression",
                        "random_forest_classifier"
                    }
                },
                {
                    MLTaskType.Regression,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "linear_regression",
                        "random_forest_regressor"
                    }
                },
                {
                    MLTaskType.Clustering,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "kmeans"
                    }
                },
                {
                    MLTaskType.AnomalyDetection,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "isolation_forest"
                    }
                }
            };

        /// <summary>
        /// Determines whether the specified algorithm key is valid for the supplied task type.
        /// </summary>
        public static bool IsSupported(MLTaskType taskType, string algorithmKey)
        {
            return !string.IsNullOrWhiteSpace(algorithmKey) &&
                   AlgorithmsByTask.ContainsKey(taskType) &&
                   AlgorithmsByTask[taskType].Contains(algorithmKey.Trim());
        }

        /// <summary>
        /// Determines whether the supplied task type requires a target column.
        /// </summary>
        public static bool RequiresTargetColumn(MLTaskType taskType)
        {
            return taskType == MLTaskType.Classification || taskType == MLTaskType.Regression;
        }
    }
}
