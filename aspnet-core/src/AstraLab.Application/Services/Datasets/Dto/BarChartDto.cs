using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents bar chart data for a categorical dataset column.
    /// </summary>
    public class BarChartDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the bar chart data.
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
        /// Gets or sets the total number of distinct non-null categories.
        /// </summary>
        public long DistinctCategoryCount { get; set; }

        /// <summary>
        /// Gets or sets the number of null values excluded from the chart.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the returned category buckets ordered for frontend charting.
        /// </summary>
        public List<BarChartCategoryDto> Categories { get; set; } = new List<BarChartCategoryDto>();
    }

    /// <summary>
    /// Represents a single category/count pair for a bar chart.
    /// </summary>
    public class BarChartCategoryDto
    {
        /// <summary>
        /// Gets or sets the category label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the category frequency.
        /// </summary>
        public long Count { get; set; }
    }
}
