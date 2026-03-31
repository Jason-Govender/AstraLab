namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a request for distribution analysis data.
    /// </summary>
    public class GetDistributionAnalysisRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the chart column.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier to analyze.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional histogram bucket count for numeric distributions.
        /// </summary>
        public int? BucketCount { get; set; }

        /// <summary>
        /// Gets or sets the optional top-category count for categorical distributions.
        /// </summary>
        public int? TopCategoryCount { get; set; }
    }
}
