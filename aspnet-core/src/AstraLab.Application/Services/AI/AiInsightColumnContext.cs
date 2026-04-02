namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents a compact high-signal profiled column insight.
    /// </summary>
    public class AiInsightColumnContext
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
        /// Gets or sets the inferred data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the null count.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the null percentage.
        /// </summary>
        public decimal NullPercentage { get; set; }

        /// <summary>
        /// Gets or sets the distinct value count.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets whether the column contains anomalies.
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
        /// Gets or sets the mean when available.
        /// </summary>
        public decimal? Mean { get; set; }

        /// <summary>
        /// Gets or sets the minimum when available.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum when available.
        /// </summary>
        public decimal? Max { get; set; }
    }
}
