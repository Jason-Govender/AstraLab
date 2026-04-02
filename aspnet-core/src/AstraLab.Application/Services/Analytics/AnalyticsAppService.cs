using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Services.Analytics.Dto;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Exposes read-only analytics and reporting retrieval workflows.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class AnalyticsAppService : AstraLabAppServiceBase, IAnalyticsAppService
    {
        private readonly IAnalyticsReportGenerator _analyticsReportGenerator;
        private readonly IAnalyticsExportGenerator _analyticsExportGenerator;
        private readonly IAnalyticsSummaryBuilder _analyticsSummaryBuilder;
        private readonly IRepository<InsightRecord, long> _insightRecordRepository;
        private readonly IRepository<ReportRecord, long> _reportRecordRepository;
        private readonly IRepository<AnalyticsExport, long> _analyticsExportRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsAppService"/> class.
        /// </summary>
        public AnalyticsAppService(
            IAnalyticsReportGenerator analyticsReportGenerator,
            IAnalyticsExportGenerator analyticsExportGenerator,
            IAnalyticsSummaryBuilder analyticsSummaryBuilder,
            IRepository<InsightRecord, long> insightRecordRepository,
            IRepository<ReportRecord, long> reportRecordRepository,
            IRepository<AnalyticsExport, long> analyticsExportRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _analyticsReportGenerator = analyticsReportGenerator;
            _analyticsExportGenerator = analyticsExportGenerator;
            _analyticsSummaryBuilder = analyticsSummaryBuilder;
            _insightRecordRepository = insightRecordRepository;
            _reportRecordRepository = reportRecordRepository;
            _analyticsExportRepository = analyticsExportRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Generates and persists a stakeholder-facing analytics report for the selected dataset version.
        /// </summary>
        public async Task<GeneratedDatasetReportResultDto> GenerateDatasetReportAsync(GenerateDatasetReportRequest input)
        {
            var generatedReport = await _analyticsReportGenerator.GenerateAsync(input.DatasetVersionId, GetRequiredTenantId(), AbpSession.GetUserId());

            return new GeneratedDatasetReportResultDto
            {
                DatasetVersionId = generatedReport.ReportRecord.DatasetVersionId,
                MLExperimentId = generatedReport.ReportRecord.MLExperimentId,
                Report = ObjectMapper.Map<ReportRecordDto>(generatedReport.ReportRecord)
            };
        }

        /// <summary>
        /// Generates and persists a PDF export for the selected dataset version report workflow.
        /// </summary>
        public async Task<GeneratedAnalyticsExportResultDto> ExportDatasetReportPdfAsync(ExportDatasetReportPdfRequest input)
        {
            var analyticsExport = await _analyticsExportGenerator.ExportReportPdfAsync(
                input.DatasetVersionId,
                input.ReportRecordId,
                GetRequiredTenantId(),
                AbpSession.GetUserId());

            await CurrentUnitOfWork.SaveChangesAsync();
            var report = await GetValidatedReportAsync(analyticsExport.ReportRecordId.Value, GetRequiredTenantId(), AbpSession.GetUserId());

            return new GeneratedAnalyticsExportResultDto
            {
                DatasetVersionId = analyticsExport.DatasetVersionId,
                MLExperimentId = analyticsExport.MLExperimentId,
                Report = ObjectMapper.Map<ReportRecordDto>(report),
                Export = ObjectMapper.Map<AnalyticsExportDto>(analyticsExport)
            };
        }

        /// <summary>
        /// Generates and persists a CSV export of structured analytics highlights for the selected dataset version.
        /// </summary>
        public async Task<GeneratedAnalyticsExportResultDto> ExportDatasetInsightsCsvAsync(ExportDatasetInsightsCsvRequest input)
        {
            var analyticsExport = await _analyticsExportGenerator.ExportInsightsCsvAsync(
                input.DatasetVersionId,
                input.ReportRecordId,
                GetRequiredTenantId(),
                AbpSession.GetUserId());

            await CurrentUnitOfWork.SaveChangesAsync();
            var report = await GetValidatedReportAsync(analyticsExport.ReportRecordId.Value, GetRequiredTenantId(), AbpSession.GetUserId());

            return new GeneratedAnalyticsExportResultDto
            {
                DatasetVersionId = analyticsExport.DatasetVersionId,
                MLExperimentId = analyticsExport.MLExperimentId,
                Report = ObjectMapper.Map<ReportRecordDto>(report),
                Export = ObjectMapper.Map<AnalyticsExportDto>(analyticsExport)
            };
        }

        /// <summary>
        /// Gets the unified analytics summary for the selected dataset version.
        /// </summary>
        public async Task<DatasetAnalyticsSummaryDto> GetDatasetAnalyticsSummaryAsync(EntityDto<long> datasetVersionId)
        {
            return await _analyticsSummaryBuilder.BuildAsync(datasetVersionId.Id, GetRequiredTenantId(), AbpSession.GetUserId());
        }

        /// <summary>
        /// Gets the compact dashboard analytics summary for the selected dataset version.
        /// </summary>
        public async Task<AnalyticsDashboardSummaryDto> GetDatasetDashboardSummaryAsync(EntityDto<long> datasetVersionId)
        {
            return await _analyticsSummaryBuilder.BuildDashboardAsync(datasetVersionId.Id, GetRequiredTenantId(), AbpSession.GetUserId());
        }

        /// <summary>
        /// Gets a persisted analytics insight.
        /// </summary>
        public async Task<InsightRecordDto> GetInsightAsync(EntityDto<long> id)
        {
            var insight = await GetValidatedInsightAsync(id.Id, GetRequiredTenantId(), AbpSession.GetUserId());
            return ObjectMapper.Map<InsightRecordDto>(insight);
        }

        /// <summary>
        /// Gets persisted analytics insights for the selected dataset context.
        /// </summary>
        public async Task<PagedResultDto<InsightRecordDto>> GetInsightsAsync(GetInsightsRequest input)
        {
            await ValidateDatasetVersionAccessAsync(input.DatasetVersionId);

            var tenantId = GetRequiredTenantId();
            var query = _insightRecordRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.DatasetVersionId)
                .WhereIf(input.DatasetProfileId.HasValue, item => item.DatasetProfileId == input.DatasetProfileId)
                .WhereIf(input.MLExperimentId.HasValue, item => item.MLExperimentId == input.MLExperimentId)
                .WhereIf(input.InsightType.HasValue, item => item.InsightType == input.InsightType.Value)
                .WhereIf(input.InsightSourceType.HasValue, item => item.InsightSourceType == input.InsightSourceType.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<InsightRecordDto>(
                totalCount,
                items.Select(ObjectMapper.Map<InsightRecordDto>).ToList());
        }

        /// <summary>
        /// Gets a persisted stakeholder report.
        /// </summary>
        public async Task<ReportRecordDto> GetReportAsync(EntityDto<long> id)
        {
            var report = await GetValidatedReportAsync(id.Id, GetRequiredTenantId(), AbpSession.GetUserId());
            return ObjectMapper.Map<ReportRecordDto>(report);
        }

        /// <summary>
        /// Gets persisted stakeholder reports for the selected dataset context.
        /// </summary>
        public async Task<PagedResultDto<ReportRecordDto>> GetReportsAsync(GetReportsRequest input)
        {
            await ValidateDatasetVersionAccessAsync(input.DatasetVersionId);

            var tenantId = GetRequiredTenantId();
            var query = _reportRecordRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.DatasetVersionId)
                .WhereIf(input.DatasetProfileId.HasValue, item => item.DatasetProfileId == input.DatasetProfileId)
                .WhereIf(input.MLExperimentId.HasValue, item => item.MLExperimentId == input.MLExperimentId)
                .WhereIf(input.ReportFormat.HasValue, item => item.ReportFormat == input.ReportFormat.Value)
                .WhereIf(input.ReportSourceType.HasValue, item => item.ReportSourceType == input.ReportSourceType.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ReportRecordDto>(
                totalCount,
                items.Select(ObjectMapper.Map<ReportRecordDto>).ToList());
        }

        /// <summary>
        /// Gets a persisted analytics export reference.
        /// </summary>
        public async Task<AnalyticsExportDto> GetExportAsync(EntityDto<long> id)
        {
            var analyticsExport = await GetValidatedExportAsync(id.Id, GetRequiredTenantId(), AbpSession.GetUserId());
            return ObjectMapper.Map<AnalyticsExportDto>(analyticsExport);
        }

        /// <summary>
        /// Gets persisted analytics export references for the selected dataset context.
        /// </summary>
        public async Task<PagedResultDto<AnalyticsExportDto>> GetExportsAsync(GetAnalyticsExportsRequest input)
        {
            await ValidateDatasetVersionAccessAsync(input.DatasetVersionId);

            var tenantId = GetRequiredTenantId();
            var query = _analyticsExportRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == input.DatasetVersionId)
                .WhereIf(input.MLExperimentId.HasValue, item => item.MLExperimentId == input.MLExperimentId)
                .WhereIf(input.InsightRecordId.HasValue, item => item.InsightRecordId == input.InsightRecordId)
                .WhereIf(input.ReportRecordId.HasValue, item => item.ReportRecordId == input.ReportRecordId)
                .WhereIf(input.ExportType.HasValue, item => item.ExportType == input.ExportType.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<AnalyticsExportDto>(
                totalCount,
                items.Select(ObjectMapper.Map<AnalyticsExportDto>).ToList());
        }

        /// <summary>
        /// Validates that the current user can access the selected dataset version.
        /// </summary>
        private async Task ValidateDatasetVersionAccessAsync(long datasetVersionId)
        {
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(
                datasetVersionId,
                GetRequiredTenantId(),
                AbpSession.GetUserId());
        }

        /// <summary>
        /// Gets a tenant-owned insight scoped to the current dataset owner.
        /// </summary>
        private async Task<InsightRecord> GetValidatedInsightAsync(long insightId, int tenantId, long ownerUserId)
        {
            var insight = await _insightRecordRepository.GetAll()
                .Where(item =>
                    item.Id == insightId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (insight == null)
            {
                throw new UserFriendlyException("The requested analytics insight could not be found.");
            }

            return insight;
        }

        /// <summary>
        /// Gets a tenant-owned report scoped to the current dataset owner.
        /// </summary>
        private async Task<ReportRecord> GetValidatedReportAsync(long reportId, int tenantId, long ownerUserId)
        {
            var report = await _reportRecordRepository.GetAll()
                .Where(item =>
                    item.Id == reportId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (report == null)
            {
                throw new UserFriendlyException("The requested analytics report could not be found.");
            }

            return report;
        }

        /// <summary>
        /// Gets a tenant-owned export reference scoped to the current dataset owner.
        /// </summary>
        private async Task<AnalyticsExport> GetValidatedExportAsync(long exportId, int tenantId, long ownerUserId)
        {
            var analyticsExport = await _analyticsExportRepository.GetAll()
                .Where(item =>
                    item.Id == exportId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (analyticsExport == null)
            {
                throw new UserFriendlyException("The requested analytics export could not be found.");
            }

            return analyticsExport;
        }

        /// <summary>
        /// Gets the current tenant identifier or throws when the host context is used.
        /// </summary>
        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Analytics retrieval requires a tenant context.");
            }

            return AbpSession.TenantId.Value;
        }
    }
}
