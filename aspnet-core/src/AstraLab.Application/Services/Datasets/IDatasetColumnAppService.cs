using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides dataset column application workflows for version-scoped schema metadata.
    /// </summary>
    public interface IDatasetColumnAppService : IApplicationService
    {
        /// <summary>
        /// Replaces the full column set for a dataset version.
        /// </summary>
        Task<ListResultDto<DatasetColumnDto>> ReplaceForVersionAsync(ReplaceDatasetColumnsDto input);

        /// <summary>
        /// Gets a dataset column for the current tenant.
        /// </summary>
        Task<DatasetColumnDto> GetAsync(EntityDto<long> input);

        /// <summary>
        /// Gets paged dataset columns for the current tenant and dataset version.
        /// </summary>
        Task<PagedResultDto<DatasetColumnDto>> GetAllAsync(PagedDatasetColumnResultRequestDto input);
    }
}
