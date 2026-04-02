using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.Datasets;
using AstraLab.Services.ML.Dto;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Exposes machine learning experiment workflows for tenant-owned datasets.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class MLExperimentAppService : AstraLabAppServiceBase, IMLExperimentAppService, ITransientDependency
    {
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IRepository<MLExperimentFeature, long> _mlExperimentFeatureRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IMLJobDispatcher _mlJobDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExperimentAppService"/> class.
        /// </summary>
        public MLExperimentAppService(
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<MLExperiment, long> mlExperimentRepository,
            IRepository<MLExperimentFeature, long> mlExperimentFeatureRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IMLJobDispatcher mlJobDispatcher)
        {
            _datasetColumnRepository = datasetColumnRepository;
            _mlExperimentRepository = mlExperimentRepository;
            _mlExperimentFeatureRepository = mlExperimentFeatureRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _mlJobDispatcher = mlJobDispatcher;
        }

        /// <summary>
        /// Creates and dispatches a machine learning experiment.
        /// </summary>
        public Task<MlExperimentDto> CreateAsync(CreateMlExperimentRequest input)
        {
            return CreateAndDispatchAsync(input);
        }

        /// <summary>
        /// Gets a single experiment for the current dataset owner.
        /// </summary>
        public async Task<MlExperimentDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var experiment = await BuildExperimentQuery(tenantId, ownerUserId)
                .FirstOrDefaultAsync(item => item.Id == input.Id);

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            return MapExperimentDto(experiment);
        }

        /// <summary>
        /// Lists experiments for a dataset version owned by the current user.
        /// </summary>
        public async Task<ListResultDto<MlExperimentDto>> GetByDatasetVersionAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.Id, tenantId, ownerUserId);

            var experiments = await BuildExperimentQuery(tenantId, ownerUserId)
                .Where(item => item.DatasetVersionId == input.Id)
                .OrderByDescending(item => item.CreationTime)
                .ToListAsync();

            return new ListResultDto<MlExperimentDto>(experiments.Select(MapExperimentDto).ToList());
        }

        /// <summary>
        /// Cancels a pending experiment.
        /// </summary>
        public async Task<MlExperimentDto> CancelAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var experiment = await BuildExperimentQuery(tenantId, ownerUserId)
                .FirstOrDefaultAsync(item => item.Id == input.Id);

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            if (experiment.Status != MLExperimentStatus.Pending)
            {
                throw new UserFriendlyException("Only pending ML experiments can be cancelled.");
            }

            experiment.Status = MLExperimentStatus.Cancelled;
            experiment.CompletedAtUtc = DateTime.UtcNow;
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapExperimentDto(experiment);
        }

        /// <summary>
        /// Retries an existing machine learning experiment by creating a new experiment record.
        /// </summary>
        public async Task<MlExperimentDto> RetryAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var experiment = await BuildExperimentQuery(tenantId, ownerUserId)
                .FirstOrDefaultAsync(item => item.Id == input.Id);

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            if (experiment.Status != MLExperimentStatus.Failed &&
                experiment.Status != MLExperimentStatus.Cancelled &&
                !(experiment.Status == MLExperimentStatus.Pending && !experiment.StartedAtUtc.HasValue))
            {
                throw new UserFriendlyException("Only failed, cancelled, or undispatched pending ML experiments can be retried.");
            }

            return await CreateAndDispatchAsync(new CreateMlExperimentRequest
            {
                DatasetVersionId = experiment.DatasetVersionId,
                TaskType = experiment.TaskType,
                AlgorithmKey = experiment.AlgorithmKey,
                FeatureDatasetColumnIds = experiment.SelectedFeatures
                    .OrderBy(item => item.Ordinal)
                    .Select(item => item.DatasetColumnId)
                    .ToList(),
                TargetDatasetColumnId = experiment.TargetDatasetColumnId,
                TrainingConfigurationJson = experiment.TrainingConfigurationJson
            });
        }

        private async Task<MlExperimentDto> CreateAndDispatchAsync(CreateMlExperimentRequest input)
        {
            ValidateCreateRequest(input);

            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.DatasetVersionId, tenantId, ownerUserId);
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(datasetVersion.DatasetId, tenantId, ownerUserId);

            if (datasetVersion.RawFile == null)
            {
                throw new UserFriendlyException("The selected dataset version does not have a stored dataset file for ML execution.");
            }

            if (dataset.SourceFormat != DatasetFormat.Csv && dataset.SourceFormat != DatasetFormat.Json)
            {
                throw new UserFriendlyException("Only CSV and tabular JSON dataset versions are supported for ML execution.");
            }

            if (!MLSupportedAlgorithms.IsSupported(input.TaskType, input.AlgorithmKey))
            {
                throw new UserFriendlyException("The selected ML algorithm is not supported for the requested task type.");
            }

            var normalizedTrainingConfigurationJson = NormalizeTrainingConfigurationJson(input.TrainingConfigurationJson);
            var datasetColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            if (datasetColumns.Count == 0)
            {
                throw new UserFriendlyException("Only dataset versions with persisted columns can be used for ML execution.");
            }

            var columnsById = datasetColumns.ToDictionary(item => item.Id);
            if (input.FeatureDatasetColumnIds.Any(item => !columnsById.ContainsKey(item)))
            {
                throw new UserFriendlyException("All selected feature columns must belong to the requested dataset version.");
            }

            var targetColumn = input.TargetDatasetColumnId.HasValue
                ? datasetColumns.FirstOrDefault(item => item.Id == input.TargetDatasetColumnId.Value)
                : null;

            if (MLSupportedAlgorithms.RequiresTargetColumn(input.TaskType) && targetColumn == null)
            {
                throw new UserFriendlyException("The selected ML task type requires a target column.");
            }

            if (!MLSupportedAlgorithms.RequiresTargetColumn(input.TaskType) && input.TargetDatasetColumnId.HasValue)
            {
                throw new UserFriendlyException("The selected ML task type does not support a target column.");
            }

            if (targetColumn != null && input.FeatureDatasetColumnIds.Contains(targetColumn.Id))
            {
                throw new UserFriendlyException("The selected target column cannot also be selected as a feature.");
            }

            var experiment = await _mlExperimentRepository.InsertAsync(new MLExperiment
            {
                TenantId = tenantId,
                DatasetVersionId = datasetVersion.Id,
                TargetDatasetColumnId = targetColumn?.Id,
                Status = MLExperimentStatus.Pending,
                TaskType = input.TaskType,
                AlgorithmKey = input.AlgorithmKey.Trim(),
                TrainingConfigurationJson = normalizedTrainingConfigurationJson,
                ExecutedAt = DateTime.UtcNow
            });

            await CurrentUnitOfWork.SaveChangesAsync();

            for (var index = 0; index < input.FeatureDatasetColumnIds.Count; index++)
            {
                await _mlExperimentFeatureRepository.InsertAsync(new MLExperimentFeature
                {
                    TenantId = tenantId,
                    MLExperimentId = experiment.Id,
                    DatasetColumnId = input.FeatureDatasetColumnIds[index],
                    Ordinal = index + 1
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            try
            {
                await _mlJobDispatcher.DispatchAsync(new DispatchMlExperimentRequest
                {
                    ExperimentId = experiment.Id,
                    TenantId = tenantId,
                    DatasetVersionId = datasetVersion.Id,
                    DatasetFormat = NormalizeDatasetFormat(dataset.SourceFormat),
                    DatasetStorageProvider = datasetVersion.RawFile.StorageProvider,
                    DatasetStorageKey = datasetVersion.RawFile.StorageKey,
                    TaskType = NormalizeTaskType(input.TaskType),
                    AlgorithmKey = input.AlgorithmKey.Trim(),
                    TrainingConfigurationJson = normalizedTrainingConfigurationJson,
                    FeatureColumns = input.FeatureDatasetColumnIds
                        .Select(item => columnsById[item])
                        .Select(item => new DispatchMlExperimentColumn
                        {
                            DatasetColumnId = item.Id,
                            Name = item.Name,
                            DataType = item.DataType,
                            Ordinal = item.Ordinal
                        })
                        .ToList(),
                    TargetColumn = targetColumn == null
                        ? null
                        : new DispatchMlExperimentColumn
                        {
                            DatasetColumnId = targetColumn.Id,
                            Name = targetColumn.Name,
                            DataType = targetColumn.DataType,
                            Ordinal = targetColumn.Ordinal
                        }
                });

                experiment.Status = MLExperimentStatus.Running;
                experiment.StartedAtUtc = DateTime.UtcNow;
                experiment.DispatchErrorMessage = null;
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                experiment.Status = MLExperimentStatus.Pending;
                experiment.DispatchErrorMessage = exception.Message;
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var createdExperiment = await BuildExperimentQuery(tenantId, ownerUserId)
                .FirstAsync(item => item.Id == experiment.Id);

            return MapExperimentDto(createdExperiment);
        }

        private IQueryable<MLExperiment> BuildExperimentQuery(int tenantId, long ownerUserId)
        {
            return _mlExperimentRepository.GetAll()
                .Include(item => item.TargetDatasetColumn)
                .Include(item => item.SelectedFeatures)
                    .ThenInclude(item => item.DatasetColumn)
                .Include(item => item.Model)
                    .ThenInclude(item => item.Metrics)
                .Include(item => item.Model)
                    .ThenInclude(item => item.FeatureImportances)
                        .ThenInclude(item => item.DatasetColumn)
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for ML experiment operations.");
            }

            return AbpSession.TenantId.Value;
        }

        private static void ValidateCreateRequest(CreateMlExperimentRequest input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.DatasetVersionId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version is required for ML execution.");
            }

            if (string.IsNullOrWhiteSpace(input.AlgorithmKey))
            {
                throw new UserFriendlyException("An ML algorithm must be selected.");
            }

            if (input.FeatureDatasetColumnIds == null || input.FeatureDatasetColumnIds.Count == 0)
            {
                throw new UserFriendlyException("At least one feature column must be selected.");
            }

            if (input.FeatureDatasetColumnIds.Any(item => item <= 0))
            {
                throw new UserFriendlyException("All selected feature columns must define valid dataset column identifiers.");
            }

            if (input.FeatureDatasetColumnIds.Distinct().Count() != input.FeatureDatasetColumnIds.Count)
            {
                throw new UserFriendlyException("Feature columns must not contain duplicate dataset column identifiers.");
            }
        }

        private static string NormalizeTrainingConfigurationJson(string trainingConfigurationJson)
        {
            var normalizedJson = string.IsNullOrWhiteSpace(trainingConfigurationJson)
                ? "{}"
                : trainingConfigurationJson.Trim();

            try
            {
                using (JsonDocument.Parse(normalizedJson))
                {
                    return normalizedJson;
                }
            }
            catch (JsonException)
            {
                throw new UserFriendlyException("The ML training configuration payload must be valid JSON.");
            }
        }

        private static string NormalizeDatasetFormat(DatasetFormat datasetFormat)
        {
            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return "csv";
                case DatasetFormat.Json:
                    return "json";
                default:
                    return "unknown";
            }
        }

        private static string NormalizeTaskType(MLTaskType taskType)
        {
            switch (taskType)
            {
                case MLTaskType.Classification:
                    return "classification";
                case MLTaskType.Regression:
                    return "regression";
                case MLTaskType.Clustering:
                    return "clustering";
                case MLTaskType.AnomalyDetection:
                    return "anomaly_detection";
                default:
                    return "unknown";
            }
        }

        private static MlExperimentDto MapExperimentDto(MLExperiment experiment)
        {
            return new MlExperimentDto
            {
                Id = experiment.Id,
                DatasetVersionId = experiment.DatasetVersionId,
                TaskType = experiment.TaskType,
                AlgorithmKey = experiment.AlgorithmKey,
                TargetDatasetColumnId = experiment.TargetDatasetColumnId,
                TargetColumnName = experiment.TargetDatasetColumn?.Name,
                Status = experiment.Status,
                TrainingConfigurationJson = experiment.TrainingConfigurationJson,
                ExecutedAt = experiment.ExecutedAt,
                StartedAtUtc = experiment.StartedAtUtc,
                CompletedAtUtc = experiment.CompletedAtUtc,
                FailureMessage = experiment.FailureMessage,
                DispatchErrorMessage = experiment.DispatchErrorMessage,
                WarningsJson = experiment.WarningsJson,
                CreationTime = experiment.CreationTime,
                Features = experiment.SelectedFeatures
                    .OrderBy(item => item.Ordinal)
                    .Select(item => new MlExperimentFeatureDto
                    {
                        DatasetColumnId = item.DatasetColumnId,
                        Name = item.DatasetColumn?.Name,
                        Ordinal = item.Ordinal
                    })
                    .ToList(),
                Model = experiment.Model == null
                    ? null
                    : new MlModelDto
                    {
                        ModelType = experiment.Model.ModelType,
                        ArtifactStorageProvider = experiment.Model.ArtifactStorageProvider,
                        ArtifactStorageKey = experiment.Model.ArtifactStorageKey,
                        PerformanceSummaryJson = experiment.Model.PerformanceSummaryJson,
                        WarningsJson = experiment.Model.WarningsJson,
                        ArtifactDownloadUrl = BuildArtifactDownloadUrl(experiment.Id, experiment.Model.ArtifactStorageKey),
                        Metrics = experiment.Model.Metrics
                            .OrderBy(item => item.MetricName)
                            .Select(item => new MlModelMetricDto
                            {
                                MetricName = item.MetricName,
                                MetricValue = item.MetricValue
                            })
                            .ToList(),
                        FeatureImportances = experiment.Model.FeatureImportances
                            .OrderBy(item => item.Rank)
                            .Select(item => new MlModelFeatureImportanceDto
                            {
                                DatasetColumnId = item.DatasetColumnId,
                                ColumnName = item.DatasetColumn?.Name,
                                ImportanceScore = item.ImportanceScore,
                                Rank = item.Rank
                            })
                            .ToList()
                    }
            };
        }

        /// <summary>
        /// Builds the authenticated artifact download URL when a stored artifact exists.
        /// </summary>
        private static string BuildArtifactDownloadUrl(long experimentId, string artifactStorageKey)
        {
            if (string.IsNullOrWhiteSpace(artifactStorageKey))
            {
                return null;
            }

            return $"/api/services/app/ml/experiments/{experimentId}/artifact/download";
        }
    }
}
