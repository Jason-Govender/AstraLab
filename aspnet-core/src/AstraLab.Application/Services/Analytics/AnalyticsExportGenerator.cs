using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Services.Analytics.Storage;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates and persists analytics export files from reports and unified summaries.
    /// </summary>
    public class AnalyticsExportGenerator : IAnalyticsExportGenerator, ITransientDependency
    {
        private const string PdfContentType = "application/pdf";
        private const string CsvContentType = "text/csv";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IRepository<AnalyticsExport, long> _analyticsExportRepository;
        private readonly IAnalyticsReportGenerator _analyticsReportGenerator;
        private readonly IAnalyticsExportStorage _analyticsExportStorage;
        private readonly IAnalyticsReportPdfRenderer _analyticsReportPdfRenderer;
        private readonly IAnalyticsInsightsCsvExporter _analyticsInsightsCsvExporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsExportGenerator"/> class.
        /// </summary>
        public AnalyticsExportGenerator(
            IRepository<AnalyticsExport, long> analyticsExportRepository,
            IAnalyticsReportGenerator analyticsReportGenerator,
            IAnalyticsExportStorage analyticsExportStorage,
            IAnalyticsReportPdfRenderer analyticsReportPdfRenderer,
            IAnalyticsInsightsCsvExporter analyticsInsightsCsvExporter)
        {
            _analyticsExportRepository = analyticsExportRepository;
            _analyticsReportGenerator = analyticsReportGenerator;
            _analyticsExportStorage = analyticsExportStorage;
            _analyticsReportPdfRenderer = analyticsReportPdfRenderer;
            _analyticsInsightsCsvExporter = analyticsInsightsCsvExporter;
        }

        /// <summary>
        /// Generates and persists a PDF export for a dataset analytics report.
        /// </summary>
        public async Task<AnalyticsExport> ExportReportPdfAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId)
        {
            var reportContext = await _analyticsReportGenerator.GetOrGenerateAsync(datasetVersionId, reportRecordId, tenantId, ownerUserId);
            var fileName = string.Format("dataset-report-v{0}.pdf", reportContext.Summary.VersionNumber);
            var content = _analyticsReportPdfRenderer.Render(reportContext.ReportRecord.Title, reportContext.ReportRecord.Content);

            return await PersistExportAsync(
                reportContext,
                content,
                fileName,
                PdfContentType,
                AnalyticsExportType.Document);
        }

        /// <summary>
        /// Generates and persists a CSV export of structured analytics highlights.
        /// </summary>
        public async Task<AnalyticsExport> ExportInsightsCsvAsync(long datasetVersionId, long? reportRecordId, int tenantId, long ownerUserId)
        {
            var reportContext = await _analyticsReportGenerator.GetOrGenerateAsync(datasetVersionId, reportRecordId, tenantId, ownerUserId);
            var fileName = string.Format("dataset-insights-v{0}.csv", reportContext.Summary.VersionNumber);
            var content = _analyticsInsightsCsvExporter.Export(reportContext.Summary);

            return await PersistExportAsync(
                reportContext,
                content,
                fileName,
                CsvContentType,
                AnalyticsExportType.Spreadsheet);
        }

        /// <summary>
        /// Stores the export payload and persists the export reference.
        /// </summary>
        private async Task<AnalyticsExport> PersistExportAsync(
            GeneratedAnalyticsReportContext reportContext,
            byte[] content,
            string displayName,
            string contentType,
            AnalyticsExportType exportType)
        {
            var storageKey = BuildStorageKey(reportContext.ReportRecord.DatasetVersionId, reportContext.ReportRecord.Id, displayName);
            StoredAnalyticsExportResult storedExport = null;

            try
            {
                using (var stream = new MemoryStream(content, writable: false))
                {
                    storedExport = await _analyticsExportStorage.StoreAsync(new StoreAnalyticsExportRequest
                    {
                        StorageKey = storageKey,
                        Content = stream
                    });
                }

                var analyticsExport = new AnalyticsExport
                {
                    TenantId = reportContext.ReportRecord.TenantId,
                    DatasetVersionId = reportContext.ReportRecord.DatasetVersionId,
                    MLExperimentId = reportContext.ReportRecord.MLExperimentId,
                    ReportRecordId = reportContext.ReportRecord.Id,
                    ExportType = exportType,
                    DisplayName = displayName,
                    StorageProvider = storedExport.StorageProvider,
                    StorageKey = storedExport.StorageKey,
                    ContentType = contentType,
                    SizeBytes = content.LongLength,
                    ChecksumSha256 = AnalyticsChecksumHelper.ComputeSha256(content),
                    MetadataJson = BuildMetadataJson(reportContext, exportType)
                };

                analyticsExport.Id = await _analyticsExportRepository.InsertAndGetIdAsync(analyticsExport);
                return analyticsExport;
            }
            catch
            {
                if (storedExport != null)
                {
                    await TryDeleteStoredExportAsync(storedExport);
                }

                throw;
            }
        }

        /// <summary>
        /// Builds the persisted logical export storage key.
        /// </summary>
        private static string BuildStorageKey(long datasetVersionId, long reportRecordId, string displayName)
        {
            var suffix = displayName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "pdf"
                : "csv";

            return string.Format(
                "dataset-version/{0}/report/{1}/{2}-{3}-{4}",
                datasetVersionId,
                reportRecordId,
                suffix,
                DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Builds compact export-generation metadata JSON.
        /// </summary>
        private static string BuildMetadataJson(GeneratedAnalyticsReportContext reportContext, AnalyticsExportType exportType)
        {
            return JsonSerializer.Serialize(new
            {
                datasetVersionId = reportContext.ReportRecord.DatasetVersionId,
                reportRecordId = reportContext.ReportRecord.Id,
                exportType = exportType.ToString(),
                includedMlExperimentId = reportContext.ReportRecord.MLExperimentId,
                includedAutomaticInsight = reportContext.Summary?.AiFindings?.HasAutomaticInsight,
                generatedFromNewReport = reportContext.WasCreated
            }, SerializerOptions);
        }

        /// <summary>
        /// Best-effort cleanup for a stored export when persistence fails after upload.
        /// </summary>
        private async Task TryDeleteStoredExportAsync(StoredAnalyticsExportResult storedExport)
        {
            try
            {
                await _analyticsExportStorage.DeleteAsync(new DeleteAnalyticsExportRequest
                {
                    StorageProvider = storedExport.StorageProvider,
                    StorageKey = storedExport.StorageKey
                });
            }
            catch
            {
                // ignore cleanup failures so the original export error can surface
            }
        }
    }
}
