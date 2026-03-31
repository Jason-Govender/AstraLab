using System.Threading.Tasks;
using Abp.Application.Services;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides application workflows for executing dataset transformations.
    /// </summary>
    public interface IDatasetTransformationAppService : IApplicationService
    {
        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input);
    }
}
