using System.Collections.Generic;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents the input required to create a machine learning experiment.
    /// </summary>
    public class CreateMlExperimentRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier to train against.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the requested machine learning task type.
        /// </summary>
        public MLTaskType TaskType { get; set; }

        /// <summary>
        /// Gets or sets the selected algorithm key.
        /// </summary>
        public string AlgorithmKey { get; set; }

        /// <summary>
        /// Gets or sets the selected feature dataset column identifiers in stable order.
        /// </summary>
        public List<long> FeatureDatasetColumnIds { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the optional target dataset column identifier for supervised tasks.
        /// </summary>
        public long? TargetDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the serialized training configuration payload.
        /// </summary>
        public string TrainingConfigurationJson { get; set; }
    }
}
