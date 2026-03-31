using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents correlation analysis data for numeric dataset columns.
    /// </summary>
    public class CorrelationAnalysisDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the correlation data.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the numeric columns that participated in the correlation analysis.
        /// </summary>
        public List<CorrelationColumnDto> Columns { get; set; } = new List<CorrelationColumnDto>();

        /// <summary>
        /// Gets or sets the correlation pairs returned for the requested columns.
        /// </summary>
        public List<CorrelationPairDto> Pairs { get; set; } = new List<CorrelationPairDto>();
    }

    /// <summary>
    /// Represents a numeric column reference in correlation analysis output.
    /// </summary>
    public class CorrelationColumnDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents a single correlated numeric column pair.
    /// </summary>
    public class CorrelationPairDto
    {
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
        /// Gets or sets the Pearson correlation coefficient when calculable.
        /// </summary>
        public decimal? Coefficient { get; set; }

        /// <summary>
        /// Gets or sets the sample size used for the pair calculation.
        /// </summary>
        public long SampleSize { get; set; }
    }
}
