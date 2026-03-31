using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the outcome of a dataset transformation pipeline.
    /// </summary>
    public class TransformDatasetVersionResultDto
    {
        /// <summary>
        /// Gets or sets the source dataset version identifier.
        /// </summary>
        public long SourceDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the final processed dataset version identifier.
        /// </summary>
        public long FinalDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the created processed dataset versions in pipeline order.
        /// </summary>
        public List<DatasetVersionDto> CreatedVersions { get; set; } = new List<DatasetVersionDto>();

        /// <summary>
        /// Gets or sets the persisted transformation rows in pipeline order.
        /// </summary>
        public List<DatasetTransformationDto> Transformations { get; set; } = new List<DatasetTransformationDto>();

        /// <summary>
        /// Gets or sets the final processed dataset profile summary.
        /// </summary>
        public DatasetProfileSummaryDto FinalProfile { get; set; }
    }
}
