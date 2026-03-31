using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents histogram chart data for a numeric dataset column.
    /// </summary>
    public class HistogramChartDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the histogram data.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier that was charted.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the charted column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the histogram bucket count.
        /// </summary>
        public int BucketCount { get; set; }

        /// <summary>
        /// Gets or sets the number of non-null values included in the histogram.
        /// </summary>
        public long ValueCount { get; set; }

        /// <summary>
        /// Gets or sets the number of null values excluded from the histogram.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum numeric value when present.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum numeric value when present.
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Gets or sets the histogram buckets.
        /// </summary>
        public List<HistogramBucketDto> Buckets { get; set; } = new List<HistogramBucketDto>();
    }

    /// <summary>
    /// Represents a single histogram bucket.
    /// </summary>
    public class HistogramBucketDto
    {
        /// <summary>
        /// Gets or sets the bucket label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the bucket start value.
        /// </summary>
        public decimal Start { get; set; }

        /// <summary>
        /// Gets or sets the bucket end value.
        /// </summary>
        public decimal End { get; set; }

        /// <summary>
        /// Gets or sets the number of values that fall within the bucket.
        /// </summary>
        public long Count { get; set; }
    }
}
