using System.Threading.Tasks;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Orchestrates raw dataset file storage and persistence for dataset versions.
    /// </summary>
    public interface IDatasetRawFileManager
    {
        /// <summary>
        /// Stores a raw dataset file for the specified dataset version.
        /// </summary>
        Task<StoredRawDatasetFileResult> StoreForVersionAsync(StoreRawDatasetFileRequest request);
    }
}
