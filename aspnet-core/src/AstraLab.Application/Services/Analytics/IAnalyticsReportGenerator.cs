using System.Threading.Tasks;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates and persists stakeholder-facing analytics reports for dataset versions.
    /// </summary>
    public interface IAnalyticsReportGenerator
    {
        /// <summary>
        /// Generates and persists a dataset analytics report.
        /// </summary>
        Task<GeneratedAnalyticsReportContext> GenerateAsync(long datasetVersionId, int tenantId, long ownerUserId);

        /// <summary>
        /// Gets an existing report or generates a new one when no report identifier is provided.
        /// </summary>
        Task<GeneratedAnalyticsReportContext> GetOrGenerateAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId);
    }
}
