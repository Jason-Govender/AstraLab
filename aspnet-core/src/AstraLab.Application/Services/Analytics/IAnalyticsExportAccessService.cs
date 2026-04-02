using System.Threading.Tasks;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Validates and opens persisted analytics exports for authenticated downloads.
    /// </summary>
    public interface IAnalyticsExportAccessService
    {
        /// <summary>
        /// Opens a tenant-owned analytics export for download.
        /// </summary>
        Task<AnalyticsExportDownloadResult> OpenDownloadAsync(long analyticsExportId, int tenantId, long ownerUserId);
    }
}
