using System.Threading.Tasks;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Orchestrates profiling for tenant-owned dataset versions.
    /// </summary>
    public interface IDatasetProfilingManager
    {
        /// <summary>
        /// Profiles the specified dataset version and persists the current snapshot.
        /// </summary>
        Task<DatasetProfileDto> ProfileAsync(long datasetVersionId);
    }
}
