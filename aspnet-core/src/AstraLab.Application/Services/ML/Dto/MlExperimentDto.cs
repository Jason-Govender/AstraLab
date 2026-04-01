using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a machine learning experiment returned by the application layer.
    /// </summary>
    public class MlExperimentDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the machine learning task type.
        /// </summary>
        public MLTaskType TaskType { get; set; }

        /// <summary>
        /// Gets or sets the selected algorithm key.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the optional target dataset column identifier.
        /// </summary>
        public long? TargetDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional target column name.
        /// </summary>
        public string TargetColumnName { get; set; }

        /// <summary>
        /// Gets or sets the current experiment status.
        /// </summary>
        public MLExperimentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the serialized training configuration payload.
        /// </summary>
        public string TrainingConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the experiment was requested.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the executor accepted the experiment.
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the experiment reached a terminal state.
        /// </summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the optional failure message.
        /// </summary>
        public string FailureMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional dispatch error message.
        /// </summary>
        public string DispatchErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized warnings payload.
        /// </summary>
        public string WarningsJson { get; set; }

        /// <summary>
        /// Gets or sets the experiment creation timestamp.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the selected features.
        /// </summary>
        public List<MlExperimentFeatureDto> Features { get; set; } = new List<MlExperimentFeatureDto>();

        /// <summary>
        /// Gets or sets the optional trained model payload.
        /// </summary>
        public MlModelDto Model { get; set; }
    }
}
