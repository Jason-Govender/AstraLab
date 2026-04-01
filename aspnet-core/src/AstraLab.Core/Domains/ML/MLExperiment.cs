using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents a persisted machine learning experiment run for a dataset version.
    /// </summary>
    public class MLExperiment : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the algorithm key.
        /// </summary>
        public const int MaxAlgorithmKeyLength = 100;

        /// <summary>
        /// The database column type used for serialized training configuration payloads.
        /// </summary>
        public const string TrainingConfigurationJsonColumnType = "text";

        /// <summary>
        /// The database column type used for long-form failure messages.
        /// </summary>
        public const string FailureMessageColumnType = "text";

        /// <summary>
        /// The database column type used for long-form dispatch error messages.
        /// </summary>
        public const string DispatchErrorMessageColumnType = "text";

        /// <summary>
        /// The database column type used for serialized warning payloads.
        /// </summary>
        public const string WarningsJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the experiment.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that the experiment ran against.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that the experiment ran against.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional target dataset column identifier for supervised learning scenarios.
        /// </summary>
        public long? TargetDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional target dataset column for supervised learning scenarios.
        /// </summary>
        public DatasetColumn TargetDatasetColumn { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle status of the experiment run.
        /// </summary>
        public MLExperimentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the requested machine learning task type.
        /// </summary>
        public MLTaskType TaskType { get; set; }

        /// <summary>
        /// Gets or sets the selected algorithm key.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the serialized training configuration payload.
        /// </summary>
        public string TrainingConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the experiment executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the external executor accepted the experiment.
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the experiment reached a terminal state.
        /// </summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the optional failure message when the experiment fails.
        /// </summary>
        public string FailureMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional dispatch error message when the executor could not accept the job.
        /// </summary>
        public string DispatchErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized warnings payload.
        /// </summary>
        public string WarningsJson { get; set; }

        /// <summary>
        /// Gets or sets the ordered feature selection rows for the experiment.
        /// </summary>
        public ICollection<MLExperimentFeature> SelectedFeatures { get; set; } = new List<MLExperimentFeature>();

        /// <summary>
        /// Gets or sets the optional trained model output for the experiment.
        /// </summary>
        public MLModel Model { get; set; }
    }
}
