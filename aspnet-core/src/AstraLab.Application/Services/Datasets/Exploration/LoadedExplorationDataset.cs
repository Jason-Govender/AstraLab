using System.Collections.Generic;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Transformations;

namespace AstraLab.Services.Datasets.Exploration
{
    /// <summary>
    /// Represents the loaded persisted content and metadata for an explorable dataset version.
    /// </summary>
    public class LoadedExplorationDataset
    {
        /// <summary>
        /// Gets or sets the dataset version metadata.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the ordered persisted dataset columns.
        /// </summary>
        public List<DatasetColumn> Columns { get; set; } = new List<DatasetColumn>();

        /// <summary>
        /// Gets or sets the parsed shared tabular dataset content.
        /// </summary>
        public TabularDataset Dataset { get; set; }
    }
}
