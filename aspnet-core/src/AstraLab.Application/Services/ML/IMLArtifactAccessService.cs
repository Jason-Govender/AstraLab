using System.Threading.Tasks;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Validates and opens persisted ML model artifacts for authenticated downloads.
    /// </summary>
    public interface IMLArtifactAccessService
    {
        /// <summary>
        /// Opens a tenant-owned ML artifact for download.
        /// </summary>
        Task<MLArtifactDownloadResult> OpenDownloadAsync(long mlExperimentId, int tenantId, long ownerUserId);
    }
}
