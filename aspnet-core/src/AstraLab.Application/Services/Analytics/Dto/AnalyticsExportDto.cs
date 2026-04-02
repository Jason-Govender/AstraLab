using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a persisted analytics export reference returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(AnalyticsExport))]
    public class AnalyticsExportDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the export.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier that informed the export.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional insight identifier that this export was derived from.
        /// </summary>
        public long? InsightRecordId { get; set; }

        /// <summary>
        /// Gets or sets the optional report identifier that this export was derived from.
        /// </summary>
        public long? ReportRecordId { get; set; }

        /// <summary>
        /// Gets or sets the export classification.
        /// </summary>
        public AnalyticsExportType ExportType { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing display name.
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
        /// Gets or sets the optional content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the optional size in bytes.
        /// </summary>
        public long? SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the optional checksum.
        /// </summary>
        public string ChecksumSha256 { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted export reference.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the authenticated download URL for the stored export.
        /// </summary>
        public string DownloadUrl { get; set; }
    }
}
