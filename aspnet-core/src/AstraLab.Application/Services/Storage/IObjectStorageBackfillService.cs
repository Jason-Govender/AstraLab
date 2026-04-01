using System.Threading.Tasks;

namespace AstraLab.Services.Storage
{
    /// <summary>
    /// Copies legacy local filesystem dataset files and ML artifacts into the configured object-storage providers.
    /// </summary>
    public interface IObjectStorageBackfillService
    {
        /// <summary>
        /// Backfills legacy local-storage records into the configured default providers.
        /// </summary>
        Task<ObjectStorageBackfillResult> BackfillAsync(int batchSize = 100);
    }
}
