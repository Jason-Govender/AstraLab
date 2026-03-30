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
    /// Provides ingestion-oriented dataset metadata workflows.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetAppService : AstraLabAppServiceBase, IDatasetAppService
    {
        private readonly IRepository<Dataset, long> _datasetRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetAppService"/> class.
        /// </summary>
        public DatasetAppService(IRepository<Dataset, long> datasetRepository)
        {
            _datasetRepository = datasetRepository;
        }

        /// <summary>
        /// Creates a new dataset metadata record for the current tenant user.
        /// </summary>
        public async Task<DatasetDto> CreateAsync(CreateDatasetDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var dataset = ObjectMapper.Map<Dataset>(input);
            dataset.TenantId = tenantId;
            dataset.OwnerUserId = ownerUserId;
            dataset.Status = DatasetStatus.Uploaded;

            dataset = await _datasetRepository.InsertAsync(dataset);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DatasetDto>(dataset);
        }

        /// <summary>
        /// Gets a dataset for the current tenant.
        /// </summary>
        public async Task<DatasetDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();

            var dataset = await _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Id == input.Id)
                .FirstOrDefaultAsync();

            if (dataset == null)
            {
                throw new EntityNotFoundException(typeof(Dataset), input.Id);
            }

            return ObjectMapper.Map<DatasetDto>(dataset);
        }

        /// <summary>
        /// Gets paged datasets for the current tenant.
        /// </summary>
        public async Task<PagedResultDto<DatasetDto>> GetAllAsync(PagedDatasetResultRequestDto input)
        {
            var tenantId = GetRequiredTenantId();

            var query = _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                    item => item.Name.Contains(input.Keyword) || item.Description.Contains(input.Keyword))
                .WhereIf(input.Status.HasValue, item => item.Status == input.Status.Value);

            var totalCount = await query.CountAsync();

            var datasets = await query
                .OrderByDescending(item => item.CreationTime)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DatasetDto>(
                totalCount,
                ObjectMapper.Map<System.Collections.Generic.List<DatasetDto>>(datasets));
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset operations.");
            }

            return AbpSession.TenantId.Value;
        }
    }
}
