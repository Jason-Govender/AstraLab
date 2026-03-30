using System.Threading.Tasks;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Stores immutable raw dataset files and returns logical storage references.
    /// </summary>
    public interface IRawDatasetStorage
    {
        /// <summary>
        /// Stores a raw dataset file in immutable form.
        /// </summary>
        Task<StoredRawDatasetFileResult> StoreAsync(StoreRawDatasetFileRequest request);
    }
}
