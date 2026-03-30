using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
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
    /// Provides application workflows for dataset version lineage management.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetVersionAppService : AstraLabAppServiceBase, IDatasetVersionAppService
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetVersionAppService"/> class.
        /// </summary>
        public DatasetVersionAppService(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
        }

        /// <summary>
        /// Creates a new dataset version for the current tenant and promotes it to current.
        /// </summary>
        public async Task<DatasetVersionDto> CreateAsync(CreateDatasetVersionDto input)
        {
            var tenantId = GetRequiredTenantId();
            var dataset = await GetDatasetForTenantAsync(input.DatasetId, tenantId);
            var parentVersion = await ValidateAndGetParentVersionAsync(input, tenantId);
            var nextVersionNumber = await GetNextVersionNumberAsync(input.DatasetId, tenantId);

            var datasetVersion = ObjectMapper.Map<DatasetVersion>(input);
            datasetVersion.TenantId = tenantId;
            datasetVersion.VersionNumber = nextVersionNumber;
            datasetVersion.Status = DatasetVersionStatus.Active;
            datasetVersion.ParentVersionId = parentVersion?.Id;

            // keep a single active version per dataset by superseding the previous current version
            if (dataset.CurrentVersionId.HasValue)
            {
                var currentVersion = await _datasetVersionRepository.GetAll()
                    .Where(item => item.TenantId == tenantId && item.DatasetId == dataset.Id && item.Id == dataset.CurrentVersionId.Value)
                    .FirstOrDefaultAsync();

                if (currentVersion == null)
                {
                    throw new UserFriendlyException("The dataset current version could not be found.");
                }

                currentVersion.Status = DatasetVersionStatus.Superseded;
            }

            datasetVersion = await _datasetVersionRepository.InsertAsync(datasetVersion);
            await CurrentUnitOfWork.SaveChangesAsync();

            dataset.CurrentVersionId = datasetVersion.Id;
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DatasetVersionDto>(datasetVersion);
        }

        /// <summary>
        /// Gets a dataset version for the current tenant.
        /// </summary>
        public async Task<DatasetVersionDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();

            var datasetVersion = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Dataset.TenantId == tenantId && item.Id == input.Id)
                .FirstOrDefaultAsync();

            if (datasetVersion == null)
            {
                throw new EntityNotFoundException(typeof(DatasetVersion), input.Id);
            }

            return ObjectMapper.Map<DatasetVersionDto>(datasetVersion);
        }

        /// <summary>
        /// Gets paged dataset versions for the current tenant and dataset.
        /// </summary>
        public async Task<PagedResultDto<DatasetVersionDto>> GetAllAsync(PagedDatasetVersionResultRequestDto input)
        {
            var tenantId = GetRequiredTenantId();
            await GetDatasetForTenantAsync(input.DatasetId, tenantId);

            var query = _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == input.DatasetId && item.Dataset.TenantId == tenantId)
                .WhereIf(input.Status.HasValue, item => item.Status == input.Status.Value);

            var totalCount = await query.CountAsync();

            var datasetVersions = await query
                .OrderByDescending(item => item.VersionNumber)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DatasetVersionDto>(
                totalCount,
                ObjectMapper.Map<List<DatasetVersionDto>>(datasetVersions));
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset version operations.");
            }

            return AbpSession.TenantId.Value;
        }

        private async Task<Dataset> GetDatasetForTenantAsync(long datasetId, int tenantId)
        {
            var dataset = await _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Id == datasetId)
                .FirstOrDefaultAsync();

            if (dataset == null)
            {
                throw new EntityNotFoundException(typeof(Dataset), datasetId);
            }

            return dataset;
        }

        private async Task<DatasetVersion> ValidateAndGetParentVersionAsync(CreateDatasetVersionDto input, int tenantId)
        {
            if (input.VersionType == DatasetVersionType.Raw && input.ParentVersionId.HasValue)
            {
                throw new UserFriendlyException("Raw dataset versions cannot reference a parent version.");
            }

            if (input.VersionType == DatasetVersionType.Processed && !input.ParentVersionId.HasValue)
            {
                throw new UserFriendlyException("Processed dataset versions must reference a parent version.");
            }

            if (!input.ParentVersionId.HasValue)
            {
                return null;
            }

            var parentVersion = await _datasetVersionRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetId == input.DatasetId &&
                    item.Id == input.ParentVersionId.Value)
                .FirstOrDefaultAsync();

            if (parentVersion == null)
            {
                throw new UserFriendlyException("Parent dataset version was not found for the specified dataset.");
            }

            return parentVersion;
        }

        private async Task<int> GetNextVersionNumberAsync(long datasetId, int tenantId)
        {
            var versionNumbers = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == datasetId)
                .Select(item => item.VersionNumber)
                .ToListAsync();

            return versionNumbers.Count == 0 ? 1 : versionNumbers.Max() + 1;
        }
    }
}
