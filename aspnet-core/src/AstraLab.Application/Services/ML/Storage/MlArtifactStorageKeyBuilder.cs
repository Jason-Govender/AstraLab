using System;

namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Builds normalized logical storage keys for ML model artifacts.
    /// </summary>
    public static class MlArtifactStorageKeyBuilder
    {
        /// <summary>
        /// Builds the default logical artifact storage key for an experiment model file.
        /// </summary>
        public static string BuildStorageKey(int tenantId, long experimentId)
        {
            if (tenantId <= 0)
            {
                throw new ArgumentException("A valid tenant identifier is required.", nameof(tenantId));
            }

            if (experimentId <= 0)
            {
                throw new ArgumentException("A valid experiment identifier is required.", nameof(experimentId));
            }

            return string.Format("tenants/{0}/ml/experiments/{1}/model.joblib", tenantId, experimentId);
        }
    }
}
