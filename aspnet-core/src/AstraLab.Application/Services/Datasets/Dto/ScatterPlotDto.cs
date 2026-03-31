using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents scatter plot data for a numeric dataset column pair.
    /// </summary>
    public class ScatterPlotDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the scatter plot.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the X-axis dataset column identifier.
        /// </summary>
        public long XDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis dataset column identifier.
        /// </summary>
        public long YDatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the X-axis column name.
        /// </summary>
        public string XColumnName { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis column name.
        /// </summary>
        public string YColumnName { get; set; }

        /// <summary>
        /// Gets or sets the returned point count.
        /// </summary>
        public int PointCount { get; set; }

        /// <summary>
        /// Gets or sets the returned plot points.
        /// </summary>
        public List<ScatterPlotPointDto> Points { get; set; } = new List<ScatterPlotPointDto>();
    }

    /// <summary>
    /// Represents a single scatter plot point.
    /// </summary>
    public class ScatterPlotPointDto
    {
        /// <summary>
        /// Gets or sets the one-based row number that produced the point.
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets the X-axis value.
        /// </summary>
        public decimal X { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis value.
        /// </summary>
        public decimal Y { get; set; }
    }
}
