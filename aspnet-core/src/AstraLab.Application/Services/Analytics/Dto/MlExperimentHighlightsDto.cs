using System;
using System.Collections.Generic;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the latest relevant machine-learning experiment highlights for the selected dataset version.
    /// </summary>
    public class MlExperimentHighlightsDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether a completed experiment is available.
        /// </summary>
        public bool HasCompletedExperiment { get; set; }

        /// <summary>
        /// Gets or sets the latest completed experiment identifier when available.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the experiment status when available.
        /// </summary>
        public MLExperimentStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the task type when available.
        /// </summary>
        public MLTaskType? TaskType { get; set; }

        /// <summary>
        /// Gets or sets the algorithm key when available.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the model type when available.
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Gets or sets the target column name when available.
        /// </summary>
        public string TargetColumnName { get; set; }

        /// <summary>
        /// Gets or sets the selected feature count.
        /// </summary>
        public int FeatureCount { get; set; }

        /// <summary>
        /// Gets or sets the selected feature names.
        /// </summary>
        public IReadOnlyList<string> FeatureNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the compact metric rows.
        /// </summary>
        public IReadOnlyList<AnalyticsMlMetricDto> Metrics { get; set; } = new List<AnalyticsMlMetricDto>();

        /// <summary>
        /// Gets or sets the primary metric name when available.
        /// </summary>
        public string PrimaryMetricName { get; set; }

        /// <summary>
        /// Gets or sets the primary metric value when available.
        /// </summary>
        public decimal? PrimaryMetricValue { get; set; }

        /// <summary>
        /// Gets or sets the top feature-importance rows.
        /// </summary>
        public IReadOnlyList<AnalyticsMlFeatureImportanceDto> TopFeatureImportances { get; set; } = new List<AnalyticsMlFeatureImportanceDto>();

        /// <summary>
        /// Gets or sets the surfaced warnings.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the execution time when available.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether model output is available.
        /// </summary>
        public bool HasModelOutput { get; set; }
    }
}
