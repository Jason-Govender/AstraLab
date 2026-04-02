using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Profiling;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Loads compact high-signal enrichment data from persisted profiling and transformation records.
    /// </summary>
    public class AiDatasetInsightReader : AstraLabAppServiceBase, IAiDatasetInsightReader, ITransientDependency
    {
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IRepository<DatasetTransformation, long> _datasetTransformationRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiDatasetInsightReader"/> class.
        /// </summary>
        public AiDatasetInsightReader(
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IRepository<DatasetTransformation, long> datasetTransformationRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _datasetVersionRepository = datasetVersionRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _datasetTransformationRepository = datasetTransformationRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Reads compact enrichment context for the specified dataset version.
        /// </summary>
        public async Task<AiDatasetInsightContext> ReadAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var datasetProfile = await GetDatasetProfileAsync(datasetVersionId, tenantId);
            var highSignalColumns = datasetProfile == null
                ? new List<AiInsightColumnContext>()
                : await GetHighSignalColumnsAsync(datasetProfile.Id, datasetVersionId, tenantId);
            var recentTransformations = await GetRecentTransformationsAsync(datasetVersion, tenantId);

            var summary = datasetProfile == null
                ? (TotalNullCount: 0L, OverallNullPercentage: 0m, TotalAnomalyCount: 0L, OverallAnomalyPercentage: 0m)
                : DatasetProfileSerialization.ReadSummary(datasetProfile.SummaryJson);

            return new AiDatasetInsightContext
            {
                DatasetVersionId = datasetVersionId,
                DataHealthScore = datasetProfile?.DataHealthScore,
                DuplicateRowCount = datasetProfile?.DuplicateRowCount,
                TotalNullCount = datasetProfile == null ? null : summary.TotalNullCount,
                OverallNullPercentage = datasetProfile == null ? null : summary.OverallNullPercentage,
                TotalAnomalyCount = datasetProfile == null ? null : summary.TotalAnomalyCount,
                OverallAnomalyPercentage = datasetProfile == null ? null : summary.OverallAnomalyPercentage,
                HighSignalColumns = highSignalColumns,
                RecentTransformations = recentTransformations
            };
        }

        /// <summary>
        /// Gets the persisted dataset-level profile when one exists.
        /// </summary>
        private async Task<DatasetProfile> GetDatasetProfileAsync(long datasetVersionId, int tenantId)
        {
            return await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the highest-signal profiled columns for prompt enrichment.
        /// </summary>
        private async Task<IReadOnlyList<AiInsightColumnContext>> GetHighSignalColumnsAsync(long datasetProfileId, long datasetVersionId, int tenantId)
        {
            var columnProfiles = await (
                    from columnProfile in _datasetColumnProfileRepository.GetAll()
                    join datasetColumn in _datasetColumnRepository.GetAll()
                        on columnProfile.DatasetColumnId equals datasetColumn.Id
                    where columnProfile.TenantId == tenantId &&
                          columnProfile.DatasetProfileId == datasetProfileId &&
                          datasetColumn.DatasetVersionId == datasetVersionId
                    select new ColumnInsightRecord
                    {
                        DatasetColumnId = datasetColumn.Id,
                        Name = datasetColumn.Name,
                        Ordinal = datasetColumn.Ordinal,
                        DataType = columnProfile.InferredDataType ?? datasetColumn.DataType,
                        NullCount = columnProfile.NullCount,
                        DistinctCount = columnProfile.DistinctCount,
                        StatisticsJson = columnProfile.StatisticsJson
                    })
                .ToListAsync();

            return columnProfiles
                .Select(MapColumnInsight)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Ordinal)
                .Take(AiDatasetGenerationDefaults.MaxHighSignalColumns)
                .Select(item => item.Context)
                .ToList();
        }

        /// <summary>
        /// Gets recent transformation history from the selected version lineage.
        /// </summary>
        private async Task<IReadOnlyList<AiTransformationHistoryContext>> GetRecentTransformationsAsync(DatasetVersion datasetVersion, int tenantId)
        {
            var lineageVersionIds = await GetLineageVersionIdsAsync(datasetVersion, tenantId);

            return await _datasetTransformationRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    (lineageVersionIds.Contains(item.SourceDatasetVersionId) ||
                     (item.ResultDatasetVersionId.HasValue && lineageVersionIds.Contains(item.ResultDatasetVersionId.Value))))
                .OrderByDescending(item => item.ExecutedAt)
                .ThenByDescending(item => item.Id)
                .Take(AiDatasetGenerationDefaults.MaxRecentTransformations)
                .Select(item => new AiTransformationHistoryContext
                {
                    DatasetTransformationId = item.Id,
                    TransformationType = item.TransformationType,
                    SourceDatasetVersionId = item.SourceDatasetVersionId,
                    ResultDatasetVersionId = item.ResultDatasetVersionId,
                    ExecutionOrder = item.ExecutionOrder,
                    ExecutedAt = item.ExecutedAt,
                    SummaryJson = item.SummaryJson
                })
                .ToListAsync();
        }

        /// <summary>
        /// Resolves the selected version together with its ancestors inside the same dataset.
        /// </summary>
        private async Task<HashSet<long>> GetLineageVersionIdsAsync(DatasetVersion datasetVersion, int tenantId)
        {
            var versionGraph = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == datasetVersion.DatasetId)
                .Select(item => new
                {
                    item.Id,
                    item.ParentVersionId
                })
                .ToListAsync();

            var parentLookup = versionGraph.ToDictionary(item => item.Id, item => item.ParentVersionId);
            var output = new HashSet<long>();
            long? currentVersionId = datasetVersion.Id;

            while (currentVersionId.HasValue && output.Add(currentVersionId.Value))
            {
                currentVersionId = parentLookup.TryGetValue(currentVersionId.Value, out var parentVersionId)
                    ? parentVersionId
                    : null;
            }

            return output;
        }

        /// <summary>
        /// Maps a profiled column row into a ranked high-signal column context.
        /// </summary>
        private static RankedColumnInsight MapColumnInsight(ColumnInsightRecord record)
        {
            var statistics = DatasetProfileSerialization.ReadColumnStatistics(record.StatisticsJson);
            decimal score = (statistics.NullPercentage * 1000m) + (statistics.AnomalyPercentage * 100m);

            if (statistics.HasAnomalies)
            {
                score += 100000m;
            }

            if (record.DistinctCount.HasValue)
            {
                score += Math.Min(record.DistinctCount.Value, 1000L) / 10m;
            }

            if (statistics.Mean.HasValue || statistics.Min.HasValue || statistics.Max.HasValue)
            {
                score += 25m;
            }

            return new RankedColumnInsight
            {
                Ordinal = record.Ordinal,
                Score = score,
                Context = new AiInsightColumnContext
                {
                    DatasetColumnId = record.DatasetColumnId,
                    Name = record.Name,
                    DataType = record.DataType,
                    NullCount = record.NullCount,
                    NullPercentage = statistics.NullPercentage,
                    DistinctCount = record.DistinctCount,
                    HasAnomalies = statistics.HasAnomalies,
                    AnomalyCount = statistics.AnomalyCount,
                    AnomalyPercentage = statistics.AnomalyPercentage,
                    Mean = statistics.Mean,
                    Min = statistics.Min,
                    Max = statistics.Max
                }
            };
        }

        /// <summary>
        /// Represents the compact column data projected from the database.
        /// </summary>
        private class ColumnInsightRecord
        {
            public long DatasetColumnId { get; set; }

            public string Name { get; set; }

            public int Ordinal { get; set; }

            public string DataType { get; set; }

            public long NullCount { get; set; }

            public long? DistinctCount { get; set; }

            public string StatisticsJson { get; set; }
        }

        /// <summary>
        /// Represents the ranked high-signal column result.
        /// </summary>
        private class RankedColumnInsight
        {
            public int Ordinal { get; set; }

            public decimal Score { get; set; }

            public AiInsightColumnContext Context { get; set; }
        }
    }
}
