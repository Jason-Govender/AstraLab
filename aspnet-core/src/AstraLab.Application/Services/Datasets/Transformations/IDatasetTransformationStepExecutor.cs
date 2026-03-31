using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes a single dataset transformation step against the shared tabular dataset model.
    /// </summary>
    public interface IDatasetTransformationStepExecutor
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        DatasetTransformationType TransformationType { get; }

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson);
    }
}
