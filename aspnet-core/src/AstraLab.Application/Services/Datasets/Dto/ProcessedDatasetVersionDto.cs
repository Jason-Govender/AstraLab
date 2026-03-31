using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the frontend-facing payload for a processed dataset version.
    /// </summary>
    public class ProcessedDatasetVersionDto
    {
        /// <summary>
        /// Gets or sets the processed dataset version details.
        /// </summary>
        public DatasetVersionDetailsDto Version { get; set; }

        /// <summary>
        /// Gets or sets the processed version columns ordered by ordinal.
        /// </summary>
        public List<DatasetColumnDto> Columns { get; set; } = new List<DatasetColumnDto>();

        /// <summary>
        /// Gets or sets the transformation that produced this processed version.
        /// </summary>
        public DatasetTransformationDto ProducedByTransformation { get; set; }
    }
}
