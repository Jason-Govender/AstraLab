using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Core.Domains.Analytics
{
    /// <summary>
    /// Represents a persisted export reference for stakeholder-facing analytics outputs.
    /// </summary>
    public class AnalyticsExport : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed display-name length.
        /// </summary>
        public const int MaxDisplayNameLength = 256;

        /// <summary>
        /// The maximum allowed content-type length.
        /// </summary>
        public const int MaxContentTypeLength = 256;

        /// <summary>
        /// The checksum length for SHA-256 hex digests.
        /// </summary>
        public const int ChecksumSha256Length = 64;

        /// <summary>
        /// The database column type used for serialized metadata payloads.
        /// </summary>
        public const string MetadataJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the export.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the export.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that anchors the export.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier that informed the export.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment that informed the export.
        /// </summary>
        public MLExperiment MLExperiment { get; set; }

        /// <summary>
        /// Gets or sets the optional parent insight identifier for the export.
        /// </summary>
        public long? InsightRecordId { get; set; }

        /// <summary>
        /// Gets or sets the optional parent insight for the export.
        /// </summary>
        public InsightRecord InsightRecord { get; set; }

        /// <summary>
        /// Gets or sets the optional parent report identifier for the export.
        /// </summary>
        public long? ReportRecordId { get; set; }

        /// <summary>
        /// Gets or sets the optional parent report for the export.
        /// </summary>
        public ReportRecord ReportRecord { get; set; }

        /// <summary>
        /// Gets or sets the export classification.
        /// </summary>
        public AnalyticsExportType ExportType { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing export display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the persisted storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted storage key.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the optional content type for the exported payload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the optional payload size in bytes.
        /// </summary>
        public long? SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the optional checksum for the exported payload.
        /// </summary>
        public string ChecksumSha256 { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }
    }
}
