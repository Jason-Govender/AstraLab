using System.Threading.Tasks;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Builds unified dataset-version-scoped analytics summaries from persisted platform outputs.
    /// </summary>
    public interface IAnalyticsSummaryBuilder
    {
        /// <summary>
        /// Builds the full unified analytics summary for the selected dataset version.
        /// </summary>
        Task<DatasetAnalyticsSummaryDto> BuildAsync(long datasetVersionId, int tenantId, long ownerUserId);

        /// <summary>
        /// Builds the compact dashboard summary for the selected dataset version.
        /// </summary>
        Task<AnalyticsDashboardSummaryDto> BuildDashboardAsync(long datasetVersionId, int tenantId, long ownerUserId);
    }
}
