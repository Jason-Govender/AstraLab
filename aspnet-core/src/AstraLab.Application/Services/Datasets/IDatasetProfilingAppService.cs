using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides application workflows for dataset profiling.
    /// </summary>
    public interface IDatasetProfilingAppService : IApplicationService
    {
        /// <summary>
        /// Profiles the specified dataset version and returns the current snapshot.
        /// </summary>
        Task<DatasetProfileDto> ProfileAsync(EntityDto<long> input);
    }
}
