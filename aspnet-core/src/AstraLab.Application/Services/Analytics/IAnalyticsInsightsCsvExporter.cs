using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Exports structured analytics highlights to CSV.
    /// </summary>
    public interface IAnalyticsInsightsCsvExporter
    {
        /// <summary>
        /// Builds a CSV payload from the supplied analytics summary.
        /// </summary>
        byte[] Export(DatasetAnalyticsSummaryDto summary);
    }
}
