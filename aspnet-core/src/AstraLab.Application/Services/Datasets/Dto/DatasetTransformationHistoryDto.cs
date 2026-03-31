using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the transformation history payload for a dataset.
    /// </summary>
    public class DatasetTransformationHistoryDto
    {
        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the current active dataset version identifier when available.
        /// </summary>
        public long? CurrentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the ordered transformation history rows for the dataset.
        /// </summary>
        public List<DatasetTransformationHistoryItemDto> Items { get; set; } = new List<DatasetTransformationHistoryItemDto>();
    }
}
