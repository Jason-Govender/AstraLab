using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides dataset version application workflows for ingestion lineage.
    /// </summary>
    public interface IDatasetVersionAppService : IApplicationService
    {
        /// <summary>
        /// Creates a new dataset version for the current tenant.
        /// </summary>
        Task<DatasetVersionDto> CreateAsync(CreateDatasetVersionDto input);

        /// <summary>
        /// Gets a dataset version for the current tenant.
        /// </summary>
        Task<DatasetVersionDto> GetAsync(EntityDto<long> input);

        /// <summary>
        /// Gets paged dataset versions for the current tenant and dataset.
        /// </summary>
        Task<PagedResultDto<DatasetVersionDto>> GetAllAsync(PagedDatasetVersionResultRequestDto input);
    }
}
