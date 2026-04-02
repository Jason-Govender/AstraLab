using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Carries the filters used to list persisted analytics export references.
    /// </summary>
    public class GetAnalyticsExportsRequest : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier to filter by.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier to filter by.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional parent insight identifier to filter by.
        /// </summary>
        public long? InsightRecordId { get; set; }

        /// <summary>
        /// Gets or sets the optional parent report identifier to filter by.
        /// </summary>
        public long? ReportRecordId { get; set; }

        /// <summary>
        /// Gets or sets the optional export-type filter.
        /// </summary>
        public AnalyticsExportType? ExportType { get; set; }
    }
}
