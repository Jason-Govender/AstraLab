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
    /// Builds a structured dataset context from persisted dataset metadata and profiling results.
    /// </summary>
    public class AiDatasetContextBuilder : AstraLabAppServiceBase, IAiDatasetContextBuilder, ITransientDependency
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiDatasetContextBuilder"/> class.
        /// </summary>
        public AiDatasetContextBuilder(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _datasetRepository = datasetRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Builds structured dataset context for the specified owner-scoped dataset version.
        /// </summary>
        public async Task<AiDatasetContext> BuildAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var dataset = await GetDatasetAsync(datasetVersion.DatasetId, tenantId, ownerUserId);
            var columns = await GetColumnsAsync(datasetVersionId, tenantId);
            var datasetProfile = await GetDatasetProfileAsync(datasetVersionId, tenantId);
            var columnProfiles = datasetProfile == null
                ? new List<ColumnProfileRecord>()
                : await GetColumnProfilesAsync(datasetProfile.ProfileId, tenantId, columns);

            var detailedColumnIds = GetDetailedColumnIds(columns.Count, columnProfiles);
            var columnContexts = BuildColumnContexts(columns, columnProfiles, detailedColumnIds);

            return new AiDatasetContext
            {
                Dataset = BuildDatasetSummary(dataset),
                Version = BuildDatasetVersionSummary(datasetVersion),
                Schema = BuildSchemaSummary(datasetVersion, columns.Count),
                Profiling = BuildProfilingSummary(datasetProfile),
                Columns = columnContexts,
                DetailedColumnCount = detailedColumnIds.Count,
                IsColumnContextPruned = columns.Count > AiDatasetContextDefaults.MaxColumnsWithFullDetail
            };
        }

        /// <summary>
        /// Gets the dataset scoped to the current owner and tenant.
        /// </summary>
        private async Task<Dataset> GetDatasetAsync(long datasetId, int tenantId, long ownerUserId)
        {
            return await _datasetRepository.GetAll()
                .Where(item => item.Id == datasetId && item.TenantId == tenantId && item.OwnerUserId == ownerUserId)
                .SingleAsync();
        }

        /// <summary>
        /// Gets the persisted columns for the selected dataset version.
        /// </summary>
        private async Task<List<ColumnRecord>> GetColumnsAsync(long datasetVersionId, int tenantId)
        {
            return await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .OrderBy(item => item.Ordinal)
                .Select(item => new ColumnRecord
                {
                    DatasetColumnId = item.Id,
                    Name = item.Name,
                    Ordinal = item.Ordinal,
                    DataType = item.DataType,
                    IsDataTypeInferred = item.IsDataTypeInferred,
                    NullCount = item.NullCount,
                    DistinctCount = item.DistinctCount
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gets the persisted dataset-level profile when available.
        /// </summary>
        private async Task<DatasetProfileRecord> GetDatasetProfileAsync(long datasetVersionId, int tenantId)
        {
            return await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .Select(item => new DatasetProfileRecord
                {
                    ProfileId = item.Id,
                    RowCount = item.RowCount,
                    DuplicateRowCount = item.DuplicateRowCount,
                    DataHealthScore = item.DataHealthScore,
                    SummaryJson = item.SummaryJson,
                    CreationTime = item.CreationTime
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the persisted column-level profile rows for the selected dataset profile.
        /// </summary>
        private async Task<List<ColumnProfileRecord>> GetColumnProfilesAsync(long datasetProfileId, int tenantId, IReadOnlyCollection<ColumnRecord> columns)
        {
            var ordinals = columns.ToDictionary(item => item.DatasetColumnId, item => item.Ordinal);

            return (await _datasetColumnProfileRepository.GetAll()
                    .Where(item => item.TenantId == tenantId && item.DatasetProfileId == datasetProfileId)
                    .Select(item => new ColumnProfileRecord
                    {
                        DatasetColumnId = item.DatasetColumnId,
                        InferredDataType = item.InferredDataType,
                        NullCount = item.NullCount,
                        DistinctCount = item.DistinctCount,
                        StatisticsJson = item.StatisticsJson
                    })
                    .ToListAsync())
                .Select(item =>
                {
                    item.Ordinal = ordinals.TryGetValue(item.DatasetColumnId, out var ordinal) ? ordinal : int.MaxValue;
                    return item;
                })
                .ToList();
        }

        /// <summary>
        /// Chooses which columns should retain detailed profiling context when the dataset is wide.
        /// </summary>
        private static HashSet<long> GetDetailedColumnIds(int totalColumnCount, IReadOnlyCollection<ColumnProfileRecord> columnProfiles)
        {
            if (totalColumnCount <= AiDatasetContextDefaults.MaxColumnsWithFullDetail)
            {
                return columnProfiles.Select(item => item.DatasetColumnId).ToHashSet();
            }

            return columnProfiles
                .Select(BuildColumnProfileRanking)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Ordinal)
                .Take(AiDatasetContextDefaults.MaxProfiledColumnsInCompactSummary)
                .Select(item => item.DatasetColumnId)
                .ToHashSet();
        }

        /// <summary>
        /// Builds the column context collection using base metadata for all columns and detailed profiling for the selected subset.
        /// </summary>
        private static IReadOnlyList<AiColumnContext> BuildColumnContexts(
            IReadOnlyCollection<ColumnRecord> columns,
            IReadOnlyCollection<ColumnProfileRecord> columnProfiles,
            IReadOnlySet<long> detailedColumnIds)
        {
            var profileLookup = columnProfiles.ToDictionary(item => item.DatasetColumnId);

            return columns
                .OrderBy(item => item.Ordinal)
                .Select(item =>
                {
                    profileLookup.TryGetValue(item.DatasetColumnId, out var profile);
                    return BuildColumnContext(item, profile, detailedColumnIds.Contains(item.DatasetColumnId));
                })
                .ToList();
        }

        /// <summary>
        /// Builds the dataset summary context.
        /// </summary>
        private static AiDatasetSummaryContext BuildDatasetSummary(Dataset dataset)
        {
            var compactDescription = Truncate(dataset.Description, AiDatasetContextDefaults.MaxDatasetDescriptionLength);
            var compactOriginalFileName = Truncate(dataset.OriginalFileName, AiDatasetContextDefaults.MaxOriginalFileNameLength);

            return new AiDatasetSummaryContext
            {
                DatasetId = dataset.Id,
                Name = dataset.Name,
                Description = compactDescription.Value,
                DescriptionWasTruncated = compactDescription.WasTruncated,
                SourceFormat = dataset.SourceFormat,
                Status = dataset.Status,
                OwnerUserId = dataset.OwnerUserId,
                OriginalFileName = compactOriginalFileName.Value,
                OriginalFileNameWasTruncated = compactOriginalFileName.WasTruncated,
                CreationTime = dataset.CreationTime
            };
        }

        /// <summary>
        /// Builds the dataset version summary context.
        /// </summary>
        private static AiDatasetVersionContext BuildDatasetVersionSummary(DatasetVersion datasetVersion)
        {
            return new AiDatasetVersionContext
            {
                DatasetVersionId = datasetVersion.Id,
                DatasetId = datasetVersion.DatasetId,
                VersionNumber = datasetVersion.VersionNumber,
                VersionType = datasetVersion.VersionType,
                Status = datasetVersion.Status,
                ParentVersionId = datasetVersion.ParentVersionId,
                RowCount = datasetVersion.RowCount,
                ColumnCount = datasetVersion.ColumnCount,
                SizeBytes = datasetVersion.SizeBytes,
                CreationTime = datasetVersion.CreationTime
            };
        }

        /// <summary>
        /// Builds the schema summary context.
        /// </summary>
        private static AiSchemaContext BuildSchemaSummary(DatasetVersion datasetVersion, int totalColumnCount)
        {
            var schemaPreview = Truncate(datasetVersion.SchemaJson, AiDatasetContextDefaults.MaxSchemaPreviewLength);

            return new AiSchemaContext
            {
                TotalColumnCount = totalColumnCount,
                HasSchemaJson = !string.IsNullOrWhiteSpace(datasetVersion.SchemaJson),
                SchemaJsonPreview = schemaPreview.Value,
                SchemaJsonWasTruncated = schemaPreview.WasTruncated
            };
        }

        /// <summary>
        /// Builds the dataset-level profiling summary context when a profile exists.
        /// </summary>
        private static AiProfilingContext BuildProfilingSummary(DatasetProfileRecord datasetProfile)
        {
            if (datasetProfile == null)
            {
                return null;
            }

            var summary = DatasetProfileSerialization.ReadSummary(datasetProfile.SummaryJson);
            return new AiProfilingContext
            {
                ProfileId = datasetProfile.ProfileId,
                RowCount = datasetProfile.RowCount,
                DuplicateRowCount = datasetProfile.DuplicateRowCount,
                DataHealthScore = datasetProfile.DataHealthScore,
                TotalNullCount = summary.TotalNullCount,
                OverallNullPercentage = summary.OverallNullPercentage,
                TotalAnomalyCount = summary.TotalAnomalyCount,
                OverallAnomalyPercentage = summary.OverallAnomalyPercentage,
                CreationTime = datasetProfile.CreationTime
            };
        }

        /// <summary>
        /// Builds a single column context row using base metadata and optional detailed profiling data.
        /// </summary>
        private static AiColumnContext BuildColumnContext(ColumnRecord column, ColumnProfileRecord columnProfile, bool includeDetailedProfile)
        {
            var context = new AiColumnContext
            {
                DatasetColumnId = column.DatasetColumnId,
                Name = column.Name,
                Ordinal = column.Ordinal,
                DataType = column.DataType,
                IsDataTypeInferred = column.IsDataTypeInferred,
                NullCount = column.NullCount ?? columnProfile?.NullCount,
                DistinctCount = column.DistinctCount ?? columnProfile?.DistinctCount,
                HasDetailedProfile = includeDetailedProfile && columnProfile != null
            };

            if (!context.HasDetailedProfile)
            {
                return context;
            }

            var statistics = DatasetProfileSerialization.ReadColumnStatistics(columnProfile.StatisticsJson);
            context.ProfiledInferredDataType = columnProfile.InferredDataType;
            context.NullPercentage = statistics.NullPercentage;
            context.Mean = statistics.Mean;
            context.Min = statistics.Min;
            context.Max = statistics.Max;
            context.AnomalyCount = statistics.AnomalyCount;
            context.AnomalyPercentage = statistics.AnomalyPercentage;
            context.HasAnomalies = statistics.HasAnomalies;

            return context;
        }

        /// <summary>
        /// Builds a deterministic ranking used to keep the most informative profiled columns in compact mode.
        /// </summary>
        private static ColumnProfileRanking BuildColumnProfileRanking(ColumnProfileRecord columnProfile)
        {
            var statistics = DatasetProfileSerialization.ReadColumnStatistics(columnProfile.StatisticsJson);
            decimal score = 0m;

            if (statistics.HasAnomalies)
            {
                score += 100000m;
                score += statistics.AnomalyPercentage * 100m;
                score += statistics.AnomalyCount;
            }

            score += statistics.NullPercentage * 1000m;

            if (columnProfile.DistinctCount.HasValue)
            {
                score += Math.Min(columnProfile.DistinctCount.Value, 1000L) / 10m;
            }

            if (statistics.Mean.HasValue || statistics.Min.HasValue || statistics.Max.HasValue)
            {
                score += 25m;
            }

            if (string.Equals(columnProfile.InferredDataType, "integer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnProfile.InferredDataType, "decimal", StringComparison.OrdinalIgnoreCase))
            {
                score += 10m;
            }

            return new ColumnProfileRanking
            {
                DatasetColumnId = columnProfile.DatasetColumnId,
                Ordinal = columnProfile.Ordinal,
                Score = score
            };
        }

        /// <summary>
        /// Produces a compact text fragment and indicates whether truncation was required.
        /// </summary>
        private static (string Value, bool WasTruncated) Truncate(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (null, false);
            }

            if (value.Length <= maxLength)
            {
                return (value, false);
            }

            return (value.Substring(0, maxLength), true);
        }

        /// <summary>
        /// Represents the projected dataset profile shape needed by the context builder.
        /// </summary>
        private class DatasetProfileRecord
        {
            public long ProfileId { get; set; }
            public long RowCount { get; set; }
            public long DuplicateRowCount { get; set; }
            public decimal DataHealthScore { get; set; }
            public string SummaryJson { get; set; }
            public DateTime CreationTime { get; set; }
        }

        /// <summary>
        /// Represents the projected dataset column shape needed by the context builder.
        /// </summary>
        private class ColumnRecord
        {
            public long DatasetColumnId { get; set; }
            public string Name { get; set; }
            public int Ordinal { get; set; }
            public string DataType { get; set; }
            public bool IsDataTypeInferred { get; set; }
            public long? NullCount { get; set; }
            public long? DistinctCount { get; set; }
        }

        /// <summary>
        /// Represents the projected column profile shape needed by the context builder.
        /// </summary>
        private class ColumnProfileRecord
        {
            public long DatasetColumnId { get; set; }
            public string InferredDataType { get; set; }
            public long NullCount { get; set; }
            public long? DistinctCount { get; set; }
            public string StatisticsJson { get; set; }
            public int Ordinal { get; set; }
        }

        /// <summary>
        /// Represents the deterministic ranking result used during compact-mode pruning.
        /// </summary>
        private class ColumnProfileRanking
        {
            public long DatasetColumnId { get; set; }
            public int Ordinal { get; set; }
            public decimal Score { get; set; }
        }
    }
}
