using System.IO;
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

        /// <summary>
        /// Opens a previously stored raw dataset file for reading.
        /// </summary>
        Task<Stream> OpenReadAsync(OpenReadRawDatasetFileRequest request);

        /// <summary>
        /// Deletes a previously stored raw dataset file by logical reference.
        /// </summary>
        Task DeleteAsync(DeleteRawDatasetFileRequest request);
    }
}
