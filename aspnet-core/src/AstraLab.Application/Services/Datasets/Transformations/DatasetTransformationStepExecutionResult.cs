using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Represents the output of a single executed transformation step.
    /// </summary>
    public class DatasetTransformationStepExecutionResult
    {
        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType { get; set; }

        /// <summary>
        /// Gets or sets the transformed dataset state after the step executes.
        /// </summary>
        public TabularDataset Dataset { get; set; }

        /// <summary>
        /// Gets or sets the canonical serialized configuration payload.
        /// </summary>
        public string CanonicalConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the serialized step summary payload.
        /// </summary>
        public string SummaryJson { get; set; }
    }
}
