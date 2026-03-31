using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents chart configuration metadata for a dataset version.
    /// </summary>
    public class DatasetExplorationColumnsDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the returned columns.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the chart-ready column metadata.
        /// </summary>
        public List<DatasetExplorationColumnDto> Columns { get; set; } = new List<DatasetExplorationColumnDto>();
    }

    /// <summary>
    /// Represents a single exploration column with chart compatibility flags.
    /// </summary>
    public class DatasetExplorationColumnDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the column ordinal within the dataset version.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the normalized data type name.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is numeric.
        /// </summary>
        public bool IsNumeric { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is categorical.
        /// </summary>
        public bool IsCategorical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is temporal.
        /// </summary>
        public bool IsTemporal { get; set; }
    }
}
