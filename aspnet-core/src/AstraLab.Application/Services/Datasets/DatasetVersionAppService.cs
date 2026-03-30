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
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetVersionAppService"/> class.
        /// </summary>
        public DatasetVersionAppService(
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _datasetVersionRepository = datasetVersionRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Creates a new dataset version for the current tenant and promotes it to current.
        /// </summary>
        public async Task<DatasetVersionDto> CreateAsync(CreateDatasetVersionDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.DatasetId, tenantId, ownerUserId);
            var parentVersion = await ValidateAndGetParentVersionAsync(input, tenantId, ownerUserId);
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
            var ownerUserId = AbpSession.GetUserId();
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.Id, tenantId, ownerUserId);

            return ObjectMapper.Map<DatasetVersionDto>(datasetVersion);
        }

        /// <summary>
        /// Gets paged dataset versions for the current tenant and dataset.
        /// </summary>
        public async Task<PagedResultDto<DatasetVersionDto>> GetAllAsync(PagedDatasetVersionResultRequestDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.DatasetId, tenantId, ownerUserId);

            var query = _datasetVersionRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetId == input.DatasetId &&
                    item.Dataset.TenantId == tenantId &&
                    item.Dataset.OwnerUserId == ownerUserId)
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

        private async Task<DatasetVersion> ValidateAndGetParentVersionAsync(CreateDatasetVersionDto input, int tenantId, long ownerUserId)
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
                    item.Dataset.OwnerUserId == ownerUserId &&
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
