using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents detailed metadata for the selected dataset version.
    /// </summary>
    public class DatasetVersionDetailsDto : DatasetVersionDto
    {
        /// <summary>
        /// Gets or sets the raw file metadata when available for the selected version.
        /// </summary>
        public DatasetFileSummaryDto RawFile { get; set; }
    }
}
