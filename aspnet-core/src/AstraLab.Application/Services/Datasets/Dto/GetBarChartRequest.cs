namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a request for bar chart data.
    /// </summary>
    public class GetBarChartRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the chart column.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the categorical dataset column identifier to chart.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional number of top categories to return.
        /// </summary>
        public int? TopCategoryCount { get; set; }
    }
}
