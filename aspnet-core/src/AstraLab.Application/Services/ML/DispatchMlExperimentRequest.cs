using System.Collections.Generic;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Represents the normalized experiment payload sent to the ML executor.
    /// </summary>
    public class DispatchMlExperimentRequest
    {
        /// <summary>
        /// Gets or sets the experiment identifier.
        /// </summary>
        public long ExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the normalized dataset format name.
        /// </summary>
        public string DatasetFormat { get; set; }

        /// <summary>
        /// Gets or sets the dataset storage provider.
        /// </summary>
        public string DatasetStorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the dataset storage key.
        /// </summary>
        public string DatasetStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the task type name.
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// Gets or sets the selected algorithm key.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the serialized training configuration payload.
        /// </summary>
        public string TrainingConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the selected feature columns.
        /// </summary>
        public List<DispatchMlExperimentColumn> FeatureColumns { get; set; } = new List<DispatchMlExperimentColumn>();

        /// <summary>
        /// Gets or sets the optional target column.
        /// </summary>
        public DispatchMlExperimentColumn TargetColumn { get; set; }
    }
}
