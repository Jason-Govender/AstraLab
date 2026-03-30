using System;
using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents dataset-level profiling data returned by the application layer.
    /// </summary>
    public class DatasetProfileDto
    {
        /// <summary>
        /// Gets or sets the profile identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that owns the profile.
        /// </summary>
        public long DatasetVersionId { get; set; }

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
        /// Gets or sets the serialized profile summary payload.
        /// </summary>
        public string SummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the column-level profile rows for the dataset version.
        /// </summary>
        public List<DatasetColumnProfileDto> ColumnProfiles { get; set; } = new List<DatasetColumnProfileDto>();

        /// <summary>
        /// Gets or sets the creation time of the profile snapshot.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
