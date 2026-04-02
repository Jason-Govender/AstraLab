using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Carries the filters used to list persisted analytics insights.
    /// </summary>
    public class GetInsightsRequest : PagedResultRequestDto
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
        /// Gets or sets the optional insight type filter.
        /// </summary>
        public InsightType? InsightType { get; set; }

        /// <summary>
        /// Gets or sets the optional source-type filter.
        /// </summary>
        public InsightSourceType? InsightSourceType { get; set; }
    }
}
