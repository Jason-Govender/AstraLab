using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the composite payload returned for the dataset details page.
    /// </summary>
    public class DatasetDetailsDto
    {
        /// <summary>
        /// Gets or sets the dataset metadata.
        /// </summary>
        public DatasetDto Dataset { get; set; }

        /// <summary>
        /// Gets or sets the linked dataset versions ordered newest-first.
        /// </summary>
        public List<DatasetVersionSummaryDto> Versions { get; set; } = new List<DatasetVersionSummaryDto>();

        /// <summary>
        /// Gets or sets the selected dataset version details when available.
        /// </summary>
        public DatasetVersionDetailsDto SelectedVersion { get; set; }

        /// <summary>
        /// Gets or sets the selected version columns ordered by ordinal.
        /// </summary>
        public List<DatasetColumnDto> Columns { get; set; } = new List<DatasetColumnDto>();
    }
}
