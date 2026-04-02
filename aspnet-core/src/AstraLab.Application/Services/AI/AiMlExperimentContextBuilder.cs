using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds a structured machine learning experiment context from persisted experiment metadata and model results.
    /// </summary>
    public class AiMlExperimentContextBuilder : AstraLabAppServiceBase, IAiMlExperimentContextBuilder, ITransientDependency
    {
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiMlExperimentContextBuilder"/> class.
        /// </summary>
        public AiMlExperimentContextBuilder(
            IRepository<MLExperiment, long> mlExperimentRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _mlExperimentRepository = mlExperimentRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Builds structured machine learning experiment context for the specified owner-scoped experiment.
        /// </summary>
        public async Task<AiMlExperimentContext> BuildAsync(long mlExperimentId, int tenantId, long ownerUserId)
        {
            var experiment = await _mlExperimentRepository.GetAll()
                .Include(item => item.TargetDatasetColumn)
                .Include(item => item.SelectedFeatures)
                    .ThenInclude(item => item.DatasetColumn)
                .Include(item => item.Model)
                    .ThenInclude(item => item.Metrics)
                .Include(item => item.Model)
                    .ThenInclude(item => item.FeatureImportances)
                        .ThenInclude(item => item.DatasetColumn)
                .Where(item =>
                    item.Id == mlExperimentId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(experiment.DatasetVersionId, tenantId, ownerUserId);

            return new AiMlExperimentContext
            {
                MLExperimentId = experiment.Id,
                DatasetVersionId = experiment.DatasetVersionId,
                Status = experiment.Status,
                TaskType = experiment.TaskType,
                AlgorithmKey = experiment.AlgorithmKey,
                TargetColumnName = experiment.TargetDatasetColumn?.Name,
                FeatureNames = experiment.SelectedFeatures
                    .OrderBy(item => item.Ordinal)
                    .Select(item => item.DatasetColumn?.Name ?? ("Column " + item.DatasetColumnId))
                    .ToList(),
                TrainingConfigurationJson = NormalizeJson(experiment.TrainingConfigurationJson),
                Warnings = ReadWarnings(experiment),
                Metrics = experiment.Model?.Metrics
                    .OrderBy(item => item.MetricName)
                    .Select(item => new AiMlMetricContext
                    {
                        MetricName = item.MetricName,
                        MetricValue = item.MetricValue
                    })
                    .ToList() ?? new List<AiMlMetricContext>(),
                FeatureImportances = experiment.Model?.FeatureImportances
                    .OrderBy(item => item.Rank)
                    .Select(item => new AiMlFeatureImportanceContext
                    {
                        DatasetColumnId = item.DatasetColumnId,
                        ColumnName = item.DatasetColumn?.Name,
                        ImportanceScore = item.ImportanceScore,
                        Rank = item.Rank
                    })
                    .ToList() ?? new List<AiMlFeatureImportanceContext>(),
                PerformanceSummaryJson = NormalizeJson(experiment.Model?.PerformanceSummaryJson),
                ModelType = experiment.Model?.ModelType,
                HasModelOutput = experiment.Model != null
            };
        }

        /// <summary>
        /// Reads experiment and model warnings into one distinct collection.
        /// </summary>
        private static IReadOnlyList<string> ReadWarnings(MLExperiment experiment)
        {
            return ReadWarningArray(experiment.WarningsJson)
                .Concat(ReadWarningArray(experiment.Model?.WarningsJson))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Reads a serialized warning array when one is present and valid.
        /// </summary>
        private static IReadOnlyList<string> ReadWarningArray(string warningsJson)
        {
            if (string.IsNullOrWhiteSpace(warningsJson))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(warningsJson) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Normalizes long JSON blobs to compact payloads so they stay predictable in prompt context.
        /// </summary>
        private static string NormalizeJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using (var document = JsonDocument.Parse(json))
                {
                    return JsonSerializer.Serialize(document.RootElement);
                }
            }
            catch (JsonException)
            {
                return json.Trim();
            }
        }
    }
}
