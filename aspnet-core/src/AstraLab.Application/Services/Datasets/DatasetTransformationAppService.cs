using System.Threading.Tasks;
using Abp.Authorization;
using AstraLab.Authorization;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Exposes explicit workflows for executing dataset transformation pipelines.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetTransformationAppService : AstraLabAppServiceBase, IDatasetTransformationAppService
    {
        private readonly IDatasetTransformationManager _datasetTransformationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransformationAppService"/> class.
        /// </summary>
        public DatasetTransformationAppService(IDatasetTransformationManager datasetTransformationManager)
        {
            _datasetTransformationManager = datasetTransformationManager;
        }

        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        public Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input)
        {
            return _datasetTransformationManager.TransformAsync(input);
        }
    }
}
