using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides application workflows for dataset version column metadata.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetColumnAppService : AstraLabAppServiceBase, IDatasetColumnAppService
    {
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetColumnAppService"/> class.
        /// </summary>
        public DatasetColumnAppService(
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository)
        {
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
        }

        /// <summary>
        /// Replaces the full column set for a dataset version owned by the current tenant.
        /// </summary>
        public async Task<ListResultDto<DatasetColumnDto>> ReplaceForVersionAsync(ReplaceDatasetColumnsDto input)
        {
            var tenantId = GetRequiredTenantId();
            var datasetVersion = await GetDatasetVersionForTenantAsync(input.DatasetVersionId, tenantId);
            var submittedColumns = input.Columns ?? new List<ReplaceDatasetColumnItemDto>();

            ValidateReplaceRequest(submittedColumns);

            var existingColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.DatasetVersionId)
                .ToListAsync();

            foreach (var existingColumn in existingColumns)
            {
                await _datasetColumnRepository.HardDeleteAsync(existingColumn);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var submittedColumn in submittedColumns)
            {
                var datasetColumn = ObjectMapper.Map<DatasetColumn>(submittedColumn);
                datasetColumn.TenantId = tenantId;
                datasetColumn.DatasetVersionId = datasetVersion.Id;
                await _datasetColumnRepository.InsertAsync(datasetColumn);
            }

            datasetVersion.ColumnCount = submittedColumns.Count;
            await CurrentUnitOfWork.SaveChangesAsync();

            var persistedColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            return new ListResultDto<DatasetColumnDto>(ObjectMapper.Map<List<DatasetColumnDto>>(persistedColumns));
        }

        /// <summary>
        /// Gets a dataset column for the current tenant.
        /// </summary>
        public async Task<DatasetColumnDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();

            var datasetColumn = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersion.TenantId == tenantId && item.Id == input.Id)
                .FirstOrDefaultAsync();

            if (datasetColumn == null)
            {
                throw new EntityNotFoundException(typeof(DatasetColumn), input.Id);
            }

            return ObjectMapper.Map<DatasetColumnDto>(datasetColumn);
        }

        /// <summary>
        /// Gets paged dataset columns for the current tenant and dataset version.
        /// </summary>
        public async Task<PagedResultDto<DatasetColumnDto>> GetAllAsync(PagedDatasetColumnResultRequestDto input)
        {
            var tenantId = GetRequiredTenantId();
            await GetDatasetVersionForTenantAsync(input.DatasetVersionId, tenantId);

            var query = _datasetColumnRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == input.DatasetVersionId &&
                    item.DatasetVersion.TenantId == tenantId);

            var totalCount = await query.CountAsync();

            var datasetColumns = await query
                .OrderBy(item => item.Ordinal)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DatasetColumnDto>(
                totalCount,
                ObjectMapper.Map<List<DatasetColumnDto>>(datasetColumns));
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset column operations.");
            }

            return AbpSession.TenantId.Value;
        }

        private async Task<DatasetVersion> GetDatasetVersionForTenantAsync(long datasetVersionId, int tenantId)
        {
            var datasetVersion = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Id == datasetVersionId)
                .FirstOrDefaultAsync();

            if (datasetVersion == null)
            {
                throw new EntityNotFoundException(typeof(DatasetVersion), datasetVersionId);
            }

            return datasetVersion;
        }

        private static void ValidateReplaceRequest(IReadOnlyCollection<ReplaceDatasetColumnItemDto> columns)
        {
            if (columns.Any(item => item.Name.IsNullOrWhiteSpace()))
            {
                throw new UserFriendlyException("Column name is required for each dataset column.");
            }

            if (columns.Any(item => item.DataType.IsNullOrWhiteSpace()))
            {
                throw new UserFriendlyException("Column data type is required for each dataset column.");
            }

            if (columns.Any(item => item.Ordinal <= 0))
            {
                throw new UserFriendlyException("Column ordinal must be greater than zero.");
            }

            // validate the full requested schema set before any writes happen
            if (columns.GroupBy(item => item.Ordinal).Any(group => group.Count() > 1))
            {
                throw new UserFriendlyException("Column ordinals must be unique within the dataset version.");
            }
        }
    }
}
