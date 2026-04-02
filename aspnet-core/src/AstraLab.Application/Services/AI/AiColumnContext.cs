namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the per-column metadata and compact profiling insight included in AI context assembly.
    /// </summary>
    public class AiColumnContext
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the persisted column data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data type was inferred.
        /// </summary>
        public bool IsDataTypeInferred { get; set; }

        /// <summary>
        /// Gets or sets the persisted null count when known.
        /// </summary>
        public long? NullCount { get; set; }

        /// <summary>
        /// Gets or sets the persisted distinct count when known.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled inferred data type when detailed profiling context is included.
        /// </summary>
        public string ProfiledInferredDataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed profiling data is included for this column.
        /// </summary>
        public bool HasDetailedProfile { get; set; }

        /// <summary>
        /// Gets or sets the profiled null percentage when detailed profiling context is included.
        /// </summary>
        public decimal? NullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the profiled mean when available.
        /// </summary>
        public decimal? Mean { get; set; }

        /// <summary>
        /// Gets or sets the profiled minimum value when available.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the profiled maximum value when available.
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Gets or sets the profiled anomaly count when available.
        /// </summary>
        public long? AnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled anomaly percentage when available.
        /// </summary>
        public decimal? AnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anomalies were detected when detailed profiling context is included.
        /// </summary>
        public bool? HasAnomalies { get; set; }
    }
}
