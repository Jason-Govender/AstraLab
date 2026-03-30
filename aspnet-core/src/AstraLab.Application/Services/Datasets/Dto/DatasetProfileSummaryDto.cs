using System;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the frontend-facing summary of a persisted dataset profile snapshot.
    /// </summary>
    public class DatasetProfileSummaryDto
    {
        /// <summary>
        /// Gets or sets the profiled dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the persisted profile identifier.
        /// </summary>
        public long ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the total profiled row count.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the total duplicate row count.
        /// </summary>
        public long DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the computed data health score.
        /// </summary>
        public decimal DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the total null cell count across the profiled dataset.
        /// </summary>
        public long TotalNullCount { get; set; }

        /// <summary>
        /// Gets or sets the overall null percentage across the profiled dataset.
        /// </summary>
        public decimal OverallNullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the total anomaly count across numeric observations.
        /// </summary>
        public long TotalAnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the overall anomaly percentage across numeric observations.
        /// </summary>
        public decimal OverallAnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted profile snapshot.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
