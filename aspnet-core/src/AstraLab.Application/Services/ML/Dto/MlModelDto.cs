using System.Collections.Generic;

namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents the trained model payload returned for a completed experiment.
    /// </summary>
    public class MlModelDto
    {
        /// <summary>
        /// Gets or sets the model type identifier.
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Gets or sets the persisted model artifact storage provider.
        /// </summary>
        public string ArtifactStorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted model artifact storage key.
        /// </summary>
        public string ArtifactStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the serialized performance summary payload.
        /// </summary>
        public string PerformanceSummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the serialized warnings payload.
        /// </summary>
        public string WarningsJson { get; set; }

        /// <summary>
        /// Gets or sets the persisted metrics.
        /// </summary>
        public List<MlModelMetricDto> Metrics { get; set; } = new List<MlModelMetricDto>();

        /// <summary>
        /// Gets or sets the persisted feature importances.
        /// </summary>
        public List<MlModelFeatureImportanceDto> FeatureImportances { get; set; } = new List<MlModelFeatureImportanceDto>();

        /// <summary>
        /// Gets or sets the authenticated download URL for the stored model artifact.
        /// </summary>
        public string ArtifactDownloadUrl { get; set; }
    }
}
