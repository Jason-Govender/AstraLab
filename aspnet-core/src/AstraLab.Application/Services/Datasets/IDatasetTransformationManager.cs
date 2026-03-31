using System.Threading.Tasks;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Orchestrates dataset version transformation pipelines and persistence.
    /// </summary>
    public interface IDatasetTransformationManager
    {
        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input);
    }
}
