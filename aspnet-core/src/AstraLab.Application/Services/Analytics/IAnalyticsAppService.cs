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
        /// Generates and persists a stakeholder-facing analytics report for the selected dataset version.
        /// </summary>
        Task<GeneratedDatasetReportResultDto> GenerateDatasetReportAsync(GenerateDatasetReportRequest input);

        /// <summary>
        /// Generates and persists a PDF export for the selected dataset version report workflow.
        /// </summary>
        Task<GeneratedAnalyticsExportResultDto> ExportDatasetReportPdfAsync(ExportDatasetReportPdfRequest input);

        /// <summary>
        /// Generates and persists a CSV export of structured analytics highlights for the selected dataset version.
        /// </summary>
        Task<GeneratedAnalyticsExportResultDto> ExportDatasetInsightsCsvAsync(ExportDatasetInsightsCsvRequest input);

        /// <summary>
        /// Gets the unified analytics summary for the selected dataset version.
        /// </summary>
        Task<DatasetAnalyticsSummaryDto> GetDatasetAnalyticsSummaryAsync(EntityDto<long> datasetVersionId);

        /// <summary>
        /// Gets the compact dashboard analytics summary for the selected dataset version.
        /// </summary>
        Task<AnalyticsDashboardSummaryDto> GetDatasetDashboardSummaryAsync(EntityDto<long> datasetVersionId);

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
