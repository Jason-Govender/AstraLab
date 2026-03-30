using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides dataset metadata application workflows for ingestion.
    /// </summary>
    public interface IDatasetAppService : IApplicationService
    {
        /// <summary>
        /// Creates a new dataset metadata record for the current tenant user.
        /// </summary>
        Task<DatasetDto> CreateAsync(CreateDatasetDto input);

        /// <summary>
        /// Gets a dataset for the current tenant.
        /// </summary>
        Task<DatasetDto> GetAsync(EntityDto<long> input);

        /// <summary>
        /// Gets the composite dataset details payload for the details page.
        /// </summary>
        Task<DatasetDetailsDto> GetDetailsAsync(GetDatasetDetailsInput input);

        /// <summary>
        /// Gets paged datasets for the current tenant.
        /// </summary>
        Task<PagedResultDto<DatasetListItemDto>> GetAllAsync(PagedDatasetResultRequestDto input);
    }
}
