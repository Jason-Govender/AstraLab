using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.ML.Dto;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Applies ML executor callbacks to persisted experiments and models.
    /// </summary>
    public class MLExperimentExecutionManager : AstraLabAppServiceBase, IMLExperimentExecutionManager, ITransientDependency
    {
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IRepository<MLModel, long> _mlModelRepository;
        private readonly IRepository<MLModelMetric, long> _mlModelMetricRepository;
        private readonly IRepository<MLModelFeatureImportance, long> _mlModelFeatureImportanceRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExperimentExecutionManager"/> class.
        /// </summary>
        public MLExperimentExecutionManager(
            IRepository<MLExperiment, long> mlExperimentRepository,
            IRepository<MLModel, long> mlModelRepository,
            IRepository<MLModelMetric, long> mlModelMetricRepository,
            IRepository<MLModelFeatureImportance, long> mlModelFeatureImportanceRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository)
        {
            _mlExperimentRepository = mlExperimentRepository;
            _mlModelRepository = mlModelRepository;
            _mlModelMetricRepository = mlModelMetricRepository;
            _mlModelFeatureImportanceRepository = mlModelFeatureImportanceRepository;
            _datasetColumnRepository = datasetColumnRepository;
        }

        /// <summary>
        /// Persists a successful executor completion callback.
        /// </summary>
        public async Task CompleteAsync(CompleteMlExperimentCallbackRequest input)
        {
            ValidateCompletionRequest(input);

            var experiment = await _mlExperimentRepository.GetAll()
                .Include(item => item.Model)
                    .ThenInclude(item => item.Metrics)
                .Include(item => item.Model)
                    .ThenInclude(item => item.FeatureImportances)
                .FirstOrDefaultAsync(item => item.Id == input.ExperimentId);

            if (experiment == null)
            {
                throw new UserFriendlyException("The specified ML experiment could not be found.");
            }

            if (experiment.Status == MLExperimentStatus.Completed ||
                experiment.Status == MLExperimentStatus.Cancelled)
            {
                return;
            }

            var validColumnIds = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == experiment.TenantId && item.DatasetVersionId == experiment.DatasetVersionId)
                .Select(item => item.Id)
                .ToListAsync();

            if (input.FeatureImportances.Any(item => !validColumnIds.Contains(item.DatasetColumnId)))
            {
                throw new UserFriendlyException("The ML executor returned a feature importance column that does not belong to the experiment dataset version.");
            }

            if (experiment.Model == null)
            {
                var model = await _mlModelRepository.InsertAsync(new MLModel
                {
                    TenantId = experiment.TenantId,
                    MLExperimentId = experiment.Id,
                    ModelType = input.ModelType.Trim(),
                    ArtifactStorageProvider = input.ArtifactStorageProvider?.Trim(),
                    ArtifactStorageKey = input.ArtifactStorageKey?.Trim(),
                    PerformanceSummaryJson = input.PerformanceSummaryJson,
                    WarningsJson = input.WarningsJson
                });

                await CurrentUnitOfWork.SaveChangesAsync();

                foreach (var metric in input.Metrics)
                {
                    await _mlModelMetricRepository.InsertAsync(new MLModelMetric
                    {
                        TenantId = experiment.TenantId,
                        MLModelId = model.Id,
                        MetricName = metric.MetricName.Trim(),
                        MetricValue = metric.MetricValue
                    });
                }

                foreach (var featureImportance in input.FeatureImportances.OrderBy(item => item.Rank))
                {
                    await _mlModelFeatureImportanceRepository.InsertAsync(new MLModelFeatureImportance
                    {
                        TenantId = experiment.TenantId,
                        MLModelId = model.Id,
                        DatasetColumnId = featureImportance.DatasetColumnId,
                        ImportanceScore = featureImportance.ImportanceScore,
                        Rank = featureImportance.Rank
                    });
                }
            }

            experiment.Status = MLExperimentStatus.Completed;
            experiment.StartedAtUtc = input.StartedAtUtc ?? experiment.StartedAtUtc ?? DateTime.UtcNow;
            experiment.CompletedAtUtc = input.CompletedAtUtc ?? DateTime.UtcNow;
            experiment.FailureMessage = null;
            experiment.DispatchErrorMessage = null;
            experiment.WarningsJson = input.WarningsJson;

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Persists a failed executor completion callback.
        /// </summary>
        public async Task FailAsync(FailMlExperimentCallbackRequest input)
        {
            ValidateFailureRequest(input);

            var experiment = await _mlExperimentRepository.GetAll()
                .FirstOrDefaultAsync(item => item.Id == input.ExperimentId);

            if (experiment == null)
            {
                throw new UserFriendlyException("The specified ML experiment could not be found.");
            }

            if (experiment.Status == MLExperimentStatus.Completed ||
                experiment.Status == MLExperimentStatus.Failed ||
                experiment.Status == MLExperimentStatus.Cancelled)
            {
                return;
            }

            experiment.Status = MLExperimentStatus.Failed;
            experiment.StartedAtUtc = input.StartedAtUtc ?? experiment.StartedAtUtc;
            experiment.CompletedAtUtc = input.CompletedAtUtc ?? DateTime.UtcNow;
            experiment.FailureMessage = input.FailureMessage.Trim();
            experiment.DispatchErrorMessage = null;
            experiment.WarningsJson = input.WarningsJson;

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private static void ValidateCompletionRequest(CompleteMlExperimentCallbackRequest input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.ExperimentId <= 0)
            {
                throw new UserFriendlyException("A valid ML experiment identifier is required.");
            }

            if (string.IsNullOrWhiteSpace(input.ModelType))
            {
                throw new UserFriendlyException("A model type is required for a completed ML experiment.");
            }

            if (input.Metrics == null)
            {
                throw new UserFriendlyException("Completed ML experiment metrics are required.");
            }

            if (input.FeatureImportances == null)
            {
                throw new UserFriendlyException("Completed ML experiment feature importances are required.");
            }

            if (input.Metrics.Any(item => item == null || string.IsNullOrWhiteSpace(item.MetricName)))
            {
                throw new UserFriendlyException("Completed ML experiment metrics must define metric names.");
            }

            if (input.Metrics
                .GroupBy(item => item.MetricName.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
            {
                throw new UserFriendlyException("Completed ML experiment metrics must not contain duplicate metric names.");
            }

            if (input.FeatureImportances
                .GroupBy(item => item.DatasetColumnId)
                .Any(group => group.Count() > 1))
            {
                throw new UserFriendlyException("Completed ML experiment feature importances must not contain duplicate dataset column identifiers.");
            }
        }

        private static void ValidateFailureRequest(FailMlExperimentCallbackRequest input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.ExperimentId <= 0)
            {
                throw new UserFriendlyException("A valid ML experiment identifier is required.");
            }

            if (string.IsNullOrWhiteSpace(input.FailureMessage))
            {
                throw new UserFriendlyException("A failure message is required when an ML experiment fails.");
            }
        }
    }
}
