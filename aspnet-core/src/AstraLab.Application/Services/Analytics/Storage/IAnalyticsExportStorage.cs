using System.IO;
using System.Threading.Tasks;

namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Stores and retrieves persisted analytics export payloads by logical reference.
    /// </summary>
    public interface IAnalyticsExportStorage
    {
        /// <summary>
        /// Stores the supplied analytics export content.
        /// </summary>
        Task<StoredAnalyticsExportResult> StoreAsync(StoreAnalyticsExportRequest request);

        /// <summary>
        /// Opens a previously stored analytics export for reading.
        /// </summary>
        Task<Stream> OpenReadAsync(OpenReadAnalyticsExportRequest request);

        /// <summary>
        /// Deletes a previously stored analytics export.
        /// </summary>
        Task DeleteAsync(DeleteAnalyticsExportRequest request);
    }
}
