namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a compact high-risk profiled column highlight for analytics summaries.
    /// </summary>
    public class DatasetQualityColumnHighlightDto
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
        /// Gets or sets the inferred column data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the profiled null count.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled null percentage.
        /// </summary>
        public decimal NullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the profiled distinct count when available.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anomalies were detected.
        /// </summary>
        public bool HasAnomalies { get; set; }

        /// <summary>
        /// Gets or sets the anomaly count.
        /// </summary>
        public long AnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the anomaly percentage.
        /// </summary>
        public decimal AnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the mean when the column is numeric.
        /// </summary>
        public decimal? Mean { get; set; }

        /// <summary>
        /// Gets or sets the minimum when the column is numeric.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum when the column is numeric.
        /// </summary>
        public decimal? Max { get; set; }
    }
}
