namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a request for scatter plot data.
    /// </summary>
    public class GetScatterPlotRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the chart columns.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the X-axis numeric dataset column identifier.
        /// </summary>
        public long XDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis numeric dataset column identifier.
        /// </summary>
        public long YDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum point count to return.
        /// </summary>
        public int? MaxPointCount { get; set; }
    }
}
