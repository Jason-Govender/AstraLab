using System.Collections.Generic;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes ordered transformation pipelines against the shared tabular dataset model.
    /// </summary>
    public interface IDatasetTransformationPipelineExecutor
    {
        /// <summary>
        /// Executes the supplied ordered transformation steps.
        /// </summary>
        IReadOnlyList<DatasetTransformationStepExecutionResult> Execute(TabularDataset sourceDataset, IReadOnlyList<DatasetTransformationStepRequest> steps);
    }
}
