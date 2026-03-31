using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a single tabular dataset row returned for exploration.
    /// </summary>
    public class DatasetRowDto
    {
        /// <summary>
        /// Gets or sets the one-based row number within the dataset content.
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets the ordered row values aligned to the dataset version columns.
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();
    }
}
