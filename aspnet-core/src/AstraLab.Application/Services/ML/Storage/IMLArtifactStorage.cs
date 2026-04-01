using System.IO;
using System.Threading.Tasks;

namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Stores and retrieves persisted ML model artifacts by logical reference.
    /// </summary>
    public interface IMLArtifactStorage
    {
        /// <summary>
        /// Stores the supplied artifact content.
        /// </summary>
        Task<StoredMlArtifactResult> StoreAsync(StoreMlArtifactRequest request);

        /// <summary>
        /// Opens a previously stored ML artifact for reading.
        /// </summary>
        Task<Stream> OpenReadAsync(OpenReadMlArtifactRequest request);

        /// <summary>
        /// Deletes a previously stored ML artifact.
        /// </summary>
        Task DeleteAsync(DeleteMlArtifactRequest request);
    }
}
