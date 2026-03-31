using System.Threading.Tasks;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Orchestrates immutable dataset version file storage and persistence for any version type.
    /// </summary>
    public interface IDatasetVersionFileManager
    {
        /// <summary>
        /// Stores a dataset file for the specified dataset version.
        /// </summary>
        Task<StoredRawDatasetFileResult> StoreForVersionAsync(StoreRawDatasetFileRequest request);
    }
}
