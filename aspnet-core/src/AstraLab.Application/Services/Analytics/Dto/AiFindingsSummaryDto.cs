using System;
using System.Collections.Generic;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the persisted AI findings section of the unified analytics summary.
    /// </summary>
    public class AiFindingsSummaryDto
    {
        /// <summary>
        /// Gets or sets the number of persisted AI responses for the selected dataset version.
        /// </summary>
        public int StoredAiResponseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persisted analytics insight records for the selected dataset version.
        /// </summary>
        public int StoredInsightRecordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an automatic AI insight is available.
        /// </summary>
        public bool HasAutomaticInsight { get; set; }

        /// <summary>
        /// Gets or sets the latest automatic insight preview.
        /// </summary>
        public string LatestAutomaticInsightPreview { get; set; }

        /// <summary>
        /// Gets or sets the latest manual insight preview.
        /// </summary>
        public string LatestManualInsightPreview { get; set; }

        /// <summary>
        /// Gets or sets the latest recommendation preview.
        /// </summary>
        public string LatestRecommendationPreview { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the latest persisted AI or insight finding.
        /// </summary>
        public DateTime? LatestFindingTime { get; set; }

        /// <summary>
        /// Gets or sets the most recent persisted findings.
        /// </summary>
        public IReadOnlyList<AiFindingPreviewDto> RecentFindings { get; set; } = new List<AiFindingPreviewDto>();
    }
}
