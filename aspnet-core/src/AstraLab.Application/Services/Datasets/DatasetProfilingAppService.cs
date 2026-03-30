using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using AstraLab.Authorization;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Exposes explicit profiling workflows for dataset versions.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetProfilingAppService : AstraLabAppServiceBase, IDatasetProfilingAppService
    {
        private readonly IDatasetProfilingManager _datasetProfilingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetProfilingAppService"/> class.
        /// </summary>
        public DatasetProfilingAppService(IDatasetProfilingManager datasetProfilingManager)
        {
            _datasetProfilingManager = datasetProfilingManager;
        }

        /// <summary>
        /// Profiles the specified dataset version and returns the current snapshot.
        /// </summary>
        public Task<DatasetProfileDto> ProfileAsync(EntityDto<long> input)
        {
            return _datasetProfilingManager.ProfileAsync(input.Id);
        }
    }
}
