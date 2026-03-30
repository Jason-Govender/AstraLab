using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Represents the profiling output for a dataset version.
    /// </summary>
    public class ProfileDatasetVersionResult
    {
        /// <summary>
        /// Gets or sets the profiled row count.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled duplicate row count.
        /// </summary>
        public long DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled data health score.
        /// </summary>
        public decimal DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the serialized summary payload.
        /// </summary>
        public string SummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the profiled column results.
        /// </summary>
        public IReadOnlyList<ProfiledDatasetColumnResult> Columns { get; set; }
    }
}
