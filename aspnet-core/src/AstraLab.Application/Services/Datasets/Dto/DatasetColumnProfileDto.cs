using System;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents column-level profiling data returned by the application layer.
    /// </summary>
    public class DatasetColumnProfileDto
    {
        /// <summary>
        /// Gets or sets the profile row identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the owning dataset profile identifier.
        /// </summary>
        public long DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier described by the profile row.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the inferred data type for the column.
        /// </summary>
        public string InferredDataType { get; set; }

        /// <summary>
        /// Gets or sets the null count for the column.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the null percentage for the column.
        /// </summary>
        public decimal NullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the distinct count when known.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized statistics payload for the column.
        /// </summary>
        public string StatisticsJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the profile row.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
