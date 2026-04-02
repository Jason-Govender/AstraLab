using System;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the dataset-level profiling summary included in AI context assembly.
    /// </summary>
    public class AiProfilingContext
    {
        /// <summary>
        /// Gets or sets the persisted profile identifier.
        /// </summary>
        public long ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the total profiled row count.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the duplicate row count.
        /// </summary>
        public long DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the computed data health score.
        /// </summary>
        public decimal DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the total null count across the profile.
        /// </summary>
        public long TotalNullCount { get; set; }

        /// <summary>
        /// Gets or sets the overall null percentage across the profile.
        /// </summary>
        public decimal OverallNullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the total anomaly count across the profile.
        /// </summary>
        public long TotalAnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the overall anomaly percentage across the profile.
        /// </summary>
        public decimal OverallAnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the profile creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
