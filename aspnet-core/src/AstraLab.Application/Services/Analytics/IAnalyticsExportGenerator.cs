using System.Threading.Tasks;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates and persists analytics export files from report and summary data.
    /// </summary>
    public interface IAnalyticsExportGenerator
    {
        /// <summary>
        /// Generates and persists a PDF export for a dataset analytics report.
        /// </summary>
        Task<AnalyticsExport> ExportReportPdfAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId);

        /// <summary>
        /// Generates and persists a CSV export of structured analytics highlights.
        /// </summary>
        Task<AnalyticsExport> ExportInsightsCsvAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId);
    }
}
