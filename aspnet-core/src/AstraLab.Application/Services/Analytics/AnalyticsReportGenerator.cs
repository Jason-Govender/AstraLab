using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Services.Analytics.Dto;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates and persists stakeholder-facing dataset analytics reports.
    /// </summary>
    public class AnalyticsReportGenerator : IAnalyticsReportGenerator, ITransientDependency
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IRepository<ReportRecord, long> _reportRecordRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IAnalyticsSummaryBuilder _analyticsSummaryBuilder;
        private readonly IAnalyticsReportHtmlRenderer _analyticsReportHtmlRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsReportGenerator"/> class.
        /// </summary>
        public AnalyticsReportGenerator(
            IRepository<ReportRecord, long> reportRecordRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IAnalyticsSummaryBuilder analyticsSummaryBuilder,
            IAnalyticsReportHtmlRenderer analyticsReportHtmlRenderer)
        {
            _reportRecordRepository = reportRecordRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _analyticsSummaryBuilder = analyticsSummaryBuilder;
            _analyticsReportHtmlRenderer = analyticsReportHtmlRenderer;
        }

        /// <summary>
        /// Generates and persists a dataset analytics report.
        /// </summary>
        public async Task<GeneratedAnalyticsReportContext> GenerateAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);

            var summary = await _analyticsSummaryBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);
            var report = await PersistGeneratedReportAsync(summary, tenantId);

            return new GeneratedAnalyticsReportContext
            {
                Summary = summary,
                ReportRecord = report,
                WasCreated = true
            };
        }

        /// <summary>
        /// Gets an existing report or generates a new one when no report identifier is provided.
        /// </summary>
        public async Task<GeneratedAnalyticsReportContext> GetOrGenerateAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId)
        {
            if (!reportRecordId.HasValue)
            {
                return await GenerateAsync(datasetVersionId, tenantId, ownerUserId);
            }

            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var report = await GetValidatedReportAsync(reportRecordId.Value, datasetVersionId, tenantId, ownerUserId);
            var summary = await _analyticsSummaryBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);

            return new GeneratedAnalyticsReportContext
            {
                Summary = summary,
                ReportRecord = report,
                WasCreated = false
            };
        }

        /// <summary>
        /// Persists a generated report from the supplied analytics summary.
        /// </summary>
        private async Task<ReportRecord> PersistGeneratedReportAsync(DatasetAnalyticsSummaryDto summary, int tenantId)
        {
            var report = new ReportRecord
            {
                TenantId = tenantId,
                DatasetVersionId = summary.DatasetVersionId,
                DatasetProfileId = summary.QualityHighlights?.DatasetProfileId,
                MLExperimentId = summary.MlExperimentHighlights?.MLExperimentId,
                Title = string.Format("{0} Analytics Report (Version {1})", summary.DatasetName, summary.VersionNumber),
                Summary = BuildReportSummary(summary),
                Content = _analyticsReportHtmlRenderer.Render(summary),
                ReportFormat = ReportFormat.Html,
                ReportSourceType = summary.Narrative?.Status == AnalyticsNarrativeStatus.Generated
                    ? ReportSourceType.AiGenerated
                    : ReportSourceType.SystemGenerated,
                MetadataJson = BuildMetadataJson(summary)
            };

            report.Id = await _reportRecordRepository.InsertAndGetIdAsync(report);
            return report;
        }

        /// <summary>
        /// Validates and gets a tenant-owned report for the selected dataset version.
        /// </summary>
        private async Task<ReportRecord> GetValidatedReportAsync(long reportRecordId, long datasetVersionId, int tenantId, long ownerUserId)
        {
            var report = await _reportRecordRepository.GetAll()
                .Where(item =>
                    item.Id == reportRecordId &&
                    item.DatasetVersionId == datasetVersionId &&
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
        /// Builds the short report summary stored alongside the canonical report content.
        /// </summary>
        private static string BuildReportSummary(DatasetAnalyticsSummaryDto summary)
        {
            if (summary?.Narrative?.Status == AnalyticsNarrativeStatus.Generated &&
                !string.IsNullOrWhiteSpace(summary.Narrative.Content))
            {
                var firstLine = summary.Narrative.Content
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0]
                    ?.Trim();

                if (!string.IsNullOrWhiteSpace(firstLine) && !string.Equals(firstLine, "Overview", StringComparison.OrdinalIgnoreCase))
                {
                    return firstLine;
                }
            }

            if (summary?.QualityHighlights?.HasProfile ?? false)
            {
                return string.Format(
                    "{0} rows, {1} columns, data health score {2}.",
                    summary.QualityHighlights.RowCount ?? 0,
                    summary.ColumnCount ?? 0,
                    summary.QualityHighlights.DataHealthScore?.ToString("0.##") ?? "unavailable");
            }

            return "A unified stakeholder-facing analytics report for the selected dataset version.";
        }

        /// <summary>
        /// Builds compact report-generation metadata JSON.
        /// </summary>
        private static string BuildMetadataJson(DatasetAnalyticsSummaryDto summary)
        {
            return JsonSerializer.Serialize(new
            {
                datasetVersionId = summary.DatasetVersionId,
                includedMlExperimentId = summary.MlExperimentHighlights?.MLExperimentId,
                includedAutomaticInsight = summary.AiFindings?.HasAutomaticInsight,
                transformationCount = summary.TransformationOutcomes?.Count ?? 0,
                highRiskColumnCount = summary.QualityHighlights?.HighRiskColumns?.Count ?? 0,
                aiFindingCount = summary.AiFindings?.RecentFindings?.Count ?? 0
            }, SerializerOptions);
        }
    }
}
