namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a single transformation-history row with its source and result versions.
    /// </summary>
    public class DatasetTransformationHistoryItemDto
    {
        /// <summary>
        /// Gets or sets the persisted transformation metadata.
        /// </summary>
        public DatasetTransformationDto Transformation { get; set; }

        /// <summary>
        /// Gets or sets the source dataset version summary.
        /// </summary>
        public DatasetVersionSummaryDto SourceVersion { get; set; }

        /// <summary>
        /// Gets or sets the resulting dataset version summary when present.
        /// </summary>
        public DatasetVersionSummaryDto ResultVersion { get; set; }
    }
}
