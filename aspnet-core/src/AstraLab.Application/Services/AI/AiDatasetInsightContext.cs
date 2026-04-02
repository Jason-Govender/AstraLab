using System.Collections.Generic;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents compact read-only enrichment data for dataset AI prompts.
    /// </summary>
    public class AiDatasetInsightContext
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that produced the enrichment.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional data health score.
        /// </summary>
        public decimal? DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the optional duplicate row count.
        /// </summary>
        public long? DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the optional total null count.
        /// </summary>
        public long? TotalNullCount { get; set; }

        /// <summary>
        /// Gets or sets the optional overall null percentage.
        /// </summary>
        public decimal? OverallNullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the optional total anomaly count.
        /// </summary>
        public long? TotalAnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the optional overall anomaly percentage.
        /// </summary>
        public decimal? OverallAnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the high-signal column insights.
        /// </summary>
        public IReadOnlyList<AiInsightColumnContext> HighSignalColumns { get; set; }

        /// <summary>
        /// Gets or sets the recent transformation history for the current version lineage.
        /// </summary>
        public IReadOnlyList<AiTransformationHistoryContext> RecentTransformations { get; set; }
    }
}
