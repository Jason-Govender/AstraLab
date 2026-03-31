using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides application workflows for executing dataset transformations.
    /// </summary>
    public interface IDatasetTransformationAppService : IApplicationService
    {
        /// <summary>
        /// Gets the persisted transformation history for the requested dataset.
        /// </summary>
        Task<DatasetTransformationHistoryDto> GetHistoryAsync(EntityDto<long> input);

        /// <summary>
        /// Gets the detailed metadata for a processed dataset version.
        /// </summary>
        Task<ProcessedDatasetVersionDto> GetProcessedVersionAsync(EntityDto<long> input);

        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input);
    }
}
