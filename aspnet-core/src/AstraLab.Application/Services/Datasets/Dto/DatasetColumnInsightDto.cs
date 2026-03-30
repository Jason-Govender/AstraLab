using System;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a frontend-facing column insight row for a persisted dataset profile.
    /// </summary>
    public class DatasetColumnInsightDto
    {
        /// <summary>
        /// Gets or sets the profiled dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the persisted column profile identifier.
        /// </summary>
        public long ColumnProfileId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column.
        /// </summary>
        public int Ordinal { get; set; }

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
        /// Gets or sets the distinct count when it is known.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets the numeric mean when the column is numeric.
        /// </summary>
        public decimal? Mean { get; set; }

        /// <summary>
        /// Gets or sets the numeric minimum when the column is numeric.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the numeric maximum when the column is numeric.
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Gets or sets the detected anomaly count for the column.
        /// </summary>
        public long AnomalyCount { get; set; }

        /// <summary>
        /// Gets or sets the detected anomaly percentage for the column.
        /// </summary>
        public decimal AnomalyPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column contains detected anomalies.
        /// </summary>
        public bool HasAnomalies { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted column profile row.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
