using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Carries the filters used to list persisted stakeholder reports.
    /// </summary>
    public class GetReportsRequest : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier to filter by.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset profile identifier to filter by.
        /// </summary>
        public long? DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier to filter by.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional report format filter.
        /// </summary>
        public ReportFormat? ReportFormat { get; set; }

        /// <summary>
        /// Gets or sets the optional report source-type filter.
        /// </summary>
        public ReportSourceType? ReportSourceType { get; set; }
    }
}
