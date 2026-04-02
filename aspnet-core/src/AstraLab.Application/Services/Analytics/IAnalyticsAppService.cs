using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Exposes read-only analytics and reporting retrieval workflows.
    /// </summary>
    public interface IAnalyticsAppService : IApplicationService
    {
        /// <summary>
        /// Gets a persisted analytics insight.
        /// </summary>
        Task<InsightRecordDto> GetInsightAsync(EntityDto<long> id);

        /// <summary>
        /// Gets persisted analytics insights for the selected dataset context.
        /// </summary>
        Task<PagedResultDto<InsightRecordDto>> GetInsightsAsync(GetInsightsRequest input);

        /// <summary>
        /// Gets a persisted stakeholder report.
        /// </summary>
        Task<ReportRecordDto> GetReportAsync(EntityDto<long> id);

        /// <summary>
        /// Gets persisted stakeholder reports for the selected dataset context.
        /// </summary>
        Task<PagedResultDto<ReportRecordDto>> GetReportsAsync(GetReportsRequest input);

        /// <summary>
        /// Gets a persisted analytics export reference.
        /// </summary>
        Task<AnalyticsExportDto> GetExportAsync(EntityDto<long> id);

        /// <summary>
        /// Gets persisted analytics export references for the selected dataset context.
        /// </summary>
        Task<PagedResultDto<AnalyticsExportDto>> GetExportsAsync(GetAnalyticsExportsRequest input);
    }
}
