using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Renders stakeholder-facing analytics reports to canonical HTML content.
    /// </summary>
    public interface IAnalyticsReportHtmlRenderer
    {
        /// <summary>
        /// Renders the supplied analytics summary to canonical HTML.
        /// </summary>
        string Render(DatasetAnalyticsSummaryDto summary);
    }
}
