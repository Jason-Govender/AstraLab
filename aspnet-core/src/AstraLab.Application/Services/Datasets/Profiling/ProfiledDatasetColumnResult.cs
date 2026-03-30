namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Represents the profiling output for a single dataset column.
    /// </summary>
    public class ProfiledDatasetColumnResult
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
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
    }
}
