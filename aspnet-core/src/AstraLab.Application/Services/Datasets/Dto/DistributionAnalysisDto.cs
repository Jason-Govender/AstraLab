using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a distribution analysis for a dataset column.
    /// </summary>
    public class DistributionAnalysisDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the analysis.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier that was analyzed.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the analyzed column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the normalized analyzed data type name.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the number of non-null values used in the analysis.
        /// </summary>
        public long ValueCount { get; set; }

        /// <summary>
        /// Gets or sets the number of null values excluded from the analysis.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the mean value for numeric distributions when present.
        /// </summary>
        public decimal? Mean { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for numeric distributions when present.
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value for numeric distributions when present.
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Gets or sets the median value for numeric distributions when present.
        /// </summary>
        public decimal? Median { get; set; }

        /// <summary>
        /// Gets or sets the distinct non-null category count for categorical distributions when present.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets numeric distribution buckets when present.
        /// </summary>
        public List<HistogramBucketDto> Buckets { get; set; } = new List<HistogramBucketDto>();

        /// <summary>
        /// Gets or sets categorical distribution frequencies when present.
        /// </summary>
        public List<BarChartCategoryDto> Categories { get; set; } = new List<BarChartCategoryDto>();
    }
}
