using System;
using System.Collections.Generic;

namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents the completion callback payload from the ML executor.
    /// </summary>
    public class CompleteMlExperimentCallbackRequest
    {
        /// <summary>
        /// Gets or sets the experiment identifier.
        /// </summary>
        public long ExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional started-at timestamp provided by the executor.
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the optional completed-at timestamp provided by the executor.
        /// </summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the trained model type identifier.
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Gets or sets the persisted artifact storage provider.
        /// </summary>
        public string ArtifactStorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted artifact storage key.
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
        /// Gets or sets the returned model metrics.
        /// </summary>
        public List<MlExperimentCompletionMetricDto> Metrics { get; set; } = new List<MlExperimentCompletionMetricDto>();

        /// <summary>
        /// Gets or sets the returned feature importance rows.
        /// </summary>
        public List<MlExperimentCompletionFeatureImportanceDto> FeatureImportances { get; set; } = new List<MlExperimentCompletionFeatureImportanceDto>();
    }
}
