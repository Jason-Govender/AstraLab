using System.Collections.Generic;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the deterministic dataset-quality section of the unified analytics summary.
    /// </summary>
    public class DatasetQualityHighlightsDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether a persisted profile is available.
        /// </summary>
        public bool HasProfile { get; set; }

        /// <summary>
        /// Gets or sets the current dataset profile identifier when available.
        /// </summary>
        public long? DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the profiled row count when available.
        /// </summary>
        public long? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled duplicate-row count when available.
        /// </summary>
        public long? DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the data-health score when available.
        /// </summary>
        public decimal? DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the total null count when available.
        /// </summary>
        public long? TotalNullCount { get; set; }

        /// <summary>
        /// Gets or sets the overall null percentage when available.
        /// </summary>
        public decimal? OverallNullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the total anomaly count when available.
        /// </summary>
        public long? TotalAnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the overall anomaly percentage when available.
        /// </summary>
        public decimal? OverallAnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the highest-risk profiled columns.
        /// </summary>
        public IReadOnlyList<DatasetQualityColumnHighlightDto> HighRiskColumns { get; set; } = new List<DatasetQualityColumnHighlightDto>();
    }
}
