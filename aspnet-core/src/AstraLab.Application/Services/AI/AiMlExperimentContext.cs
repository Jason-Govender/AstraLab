using System.Collections.Generic;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the structured machine learning experiment context assembled for AI workflows.
    /// </summary>
    public class AiMlExperimentContext
    {
        /// <summary>
        /// Gets or sets the machine learning experiment identifier.
        /// </summary>
        public long MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that the experiment ran against.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the current experiment status.
        /// </summary>
        public MLExperimentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the machine learning task type.
        /// </summary>
        public MLTaskType TaskType { get; set; }

        /// <summary>
        /// Gets or sets the selected algorithm key.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the optional target column name.
        /// </summary>
        public string TargetColumnName { get; set; }

        /// <summary>
        /// Gets or sets the selected feature names.
        /// </summary>
        public IReadOnlyList<string> FeatureNames { get; set; }

        /// <summary>
        /// Gets or sets the compact training configuration JSON.
        /// </summary>
        public string TrainingConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the warnings surfaced by the experiment or model.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; set; }

        /// <summary>
        /// Gets or sets the structured metric summary rows.
        /// </summary>
        public IReadOnlyList<AiMlMetricContext> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the structured feature-importance rows.
        /// </summary>
        public IReadOnlyList<AiMlFeatureImportanceContext> FeatureImportances { get; set; }

        /// <summary>
        /// Gets or sets the compact performance summary JSON returned by the model.
        /// </summary>
        public string PerformanceSummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the model type returned by the executor.
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether model output is available.
        /// </summary>
        public bool HasModelOutput { get; set; }
    }
}
