using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Profiling;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Exposes explicit profiling workflows for dataset versions.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetProfilingAppService : AstraLabAppServiceBase, IDatasetProfilingAppService
    {
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IDatasetProfilingManager _datasetProfilingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetProfilingAppService"/> class.
        /// </summary>
        public DatasetProfilingAppService(
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IDatasetProfilingManager datasetProfilingManager)
        {
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _datasetProfilingManager = datasetProfilingManager;
        }

        /// <summary>
        /// Gets the current persisted profiling summary for the specified dataset version.
        /// </summary>
        public async Task<DatasetProfileSummaryDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.Id, tenantId, ownerUserId);

            var datasetProfile = await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.Id)
                .Select(item => new
                {
                    item.Id,
                    item.DatasetVersionId,
                    item.RowCount,
                    item.DuplicateRowCount,
                    item.DataHealthScore,
                    item.SummaryJson,
                    item.CreationTime
                })
                .FirstOrDefaultAsync();

            if (datasetProfile == null)
            {
                throw new EntityNotFoundException(typeof(DatasetProfile), input.Id);
            }

            var summary = DatasetProfileSerialization.ReadSummary(datasetProfile.SummaryJson);
            return new DatasetProfileSummaryDto
            {
                DatasetVersionId = datasetProfile.DatasetVersionId,
                ProfileId = datasetProfile.Id,
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
        /// Gets paged column insights for the current persisted profile of the specified dataset version.
        /// </summary>
        public async Task<PagedResultDto<DatasetColumnInsightDto>> GetColumnsAsync(PagedDatasetColumnInsightRequestDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.DatasetVersionId, tenantId, ownerUserId);

            var datasetProfile = await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.DatasetVersionId)
                .Select(item => new { item.Id })
                .FirstOrDefaultAsync();

            if (datasetProfile == null)
            {
                throw new EntityNotFoundException(typeof(DatasetProfile), input.DatasetVersionId);
            }

            var columnQuery =
                from datasetColumnProfile in _datasetColumnProfileRepository.GetAll()
                join datasetColumn in _datasetColumnRepository.GetAll()
                    on datasetColumnProfile.DatasetColumnId equals datasetColumn.Id
                where datasetColumnProfile.TenantId == tenantId
                      && datasetColumn.TenantId == tenantId
                      && datasetColumnProfile.DatasetProfileId == datasetProfile.Id
                      && datasetColumn.DatasetVersionId == input.DatasetVersionId
                select new ColumnInsightQueryResult
                {
                    ColumnProfileId = datasetColumnProfile.Id,
                    DatasetColumnId = datasetColumnProfile.DatasetColumnId,
                    Name = datasetColumn.Name,
                    Ordinal = datasetColumn.Ordinal,
                    InferredDataType = datasetColumnProfile.InferredDataType,
                    NullCount = datasetColumnProfile.NullCount,
                    DistinctCount = datasetColumnProfile.DistinctCount,
                    StatisticsJson = datasetColumnProfile.StatisticsJson,
                    CreationTime = datasetColumnProfile.CreationTime
                };

            if (!input.InferredDataType.IsNullOrWhiteSpace())
            {
                columnQuery = columnQuery.Where(item => item.InferredDataType == input.InferredDataType);
            }

            var columnInsights = await columnQuery
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            var mappedInsights = columnInsights
                .Select(MapColumnInsightDto)
                .ToList();

            if (input.HasAnomalies.HasValue)
            {
                mappedInsights = mappedInsights
                    .Where(item => item.HasAnomalies == input.HasAnomalies.Value)
                    .ToList();
            }

            var totalCount = mappedInsights.Count;
            var items = mappedInsights
                .AsQueryable()
                .PageBy(input)
                .ToList();

            return new PagedResultDto<DatasetColumnInsightDto>(totalCount, items);
        }

        /// <summary>
        /// Profiles the specified dataset version and returns the current snapshot.
        /// </summary>
        public Task<DatasetProfileDto> ProfileAsync(EntityDto<long> input)
        {
            return _datasetProfilingManager.ProfileAsync(input.Id);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset profiling operations.");
            }

            return AbpSession.TenantId.Value;
        }

        /// <summary>
        /// Maps a persisted column profile and column metadata pair into the frontend-facing insight shape.
        /// </summary>
        private static DatasetColumnInsightDto MapColumnInsightDto(ColumnInsightQueryResult source)
        {
            var statistics = DatasetProfileSerialization.ReadColumnStatistics(source.StatisticsJson);
            return new DatasetColumnInsightDto
            {
                DatasetColumnId = source.DatasetColumnId,
                ColumnProfileId = source.ColumnProfileId,
                Name = source.Name,
                Ordinal = source.Ordinal,
                InferredDataType = source.InferredDataType,
                NullCount = source.NullCount,
                NullPercentage = statistics.NullPercentage,
                DistinctCount = source.DistinctCount,
                Mean = statistics.Mean,
                Min = statistics.Min,
                Max = statistics.Max,
                AnomalyCount = statistics.AnomalyCount,
                AnomalyPercentage = statistics.AnomalyPercentage,
                HasAnomalies = statistics.HasAnomalies,
                CreationTime = source.CreationTime
            };
        }

        /// <summary>
        /// Represents the projected column insight query shape used by the profiling read API.
        /// </summary>
        private class ColumnInsightQueryResult
        {
            public long ColumnProfileId { get; set; }

            public long DatasetColumnId { get; set; }

            public string Name { get; set; }

            public int Ordinal { get; set; }

            public string InferredDataType { get; set; }

            public long NullCount { get; set; }

            public long? DistinctCount { get; set; }

            public string StatisticsJson { get; set; }

            public System.DateTime CreationTime { get; set; }
        }
    }
}
