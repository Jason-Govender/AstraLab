namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a request for histogram chart data.
    /// </summary>
    public class GetHistogramChartRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the chart column.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the numeric dataset column identifier to chart.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional requested histogram bucket count.
        /// </summary>
        public int? BucketCount { get; set; }
    }
}
