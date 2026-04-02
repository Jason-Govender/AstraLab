using AstraLab.Core.Domains.Analytics;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Carries a persisted report together with the summary used to generate it.
    /// </summary>
    public class GeneratedAnalyticsReportContext
    {
        /// <summary>
        /// Gets or sets the deterministic analytics summary used to generate the report.
        /// </summary>
        public DatasetAnalyticsSummaryDto Summary { get; set; }

        /// <summary>
        /// Gets or sets the persisted report record.
        /// </summary>
        public ReportRecord ReportRecord { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report was created during the current workflow.
        /// </summary>
        public bool WasCreated { get; set; }
    }
}
