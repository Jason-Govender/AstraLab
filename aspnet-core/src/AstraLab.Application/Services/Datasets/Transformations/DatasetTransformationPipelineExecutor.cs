using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Coordinates the execution of ordered transformation steps against the shared tabular dataset model.
    /// </summary>
    public class DatasetTransformationPipelineExecutor : IDatasetTransformationPipelineExecutor, ITransientDependency
    {
        private readonly IReadOnlyDictionary<DatasetTransformationType, IDatasetTransformationStepExecutor> _stepExecutors;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransformationPipelineExecutor"/> class.
        /// </summary>
        public DatasetTransformationPipelineExecutor(IEnumerable<IDatasetTransformationStepExecutor> stepExecutors)
        {
            _stepExecutors = stepExecutors.ToDictionary(item => item.TransformationType);
        }

        /// <summary>
        /// Executes the supplied ordered transformation steps.
        /// </summary>
        public IReadOnlyList<DatasetTransformationStepExecutionResult> Execute(TabularDataset sourceDataset, IReadOnlyList<DatasetTransformationStepRequest> steps)
        {
            if (sourceDataset == null)
            {
                throw new ArgumentNullException(nameof(sourceDataset));
            }

            if (steps == null || steps.Count == 0)
            {
                throw new UserFriendlyException("At least one transformation step is required.");
            }

            var currentDataset = sourceDataset.Clone();
            var results = new List<DatasetTransformationStepExecutionResult>();

            foreach (var step in steps)
            {
                if (!_stepExecutors.TryGetValue(step.TransformationType, out var stepExecutor))
                {
                    throw new UserFriendlyException($"The {step.TransformationType} transformation is not supported.");
                }

                var result = stepExecutor.Execute(currentDataset, step.ConfigurationJson);
                currentDataset = result.Dataset.Clone();
                results.Add(result);
            }

            return results;
        }
    }
}
