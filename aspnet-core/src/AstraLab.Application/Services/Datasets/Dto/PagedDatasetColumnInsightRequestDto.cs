using Abp.Application.Services.Dto;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a paged request for frontend-facing dataset column insights.
    /// </summary>
    public class PagedDatasetColumnInsightRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the requested insights.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets an optional anomaly filter for column insights.
        /// </summary>
        public bool? HasAnomalies { get; set; }

        /// <summary>
        /// Gets or sets an optional inferred data type filter for column insights.
        /// </summary>
        public string InferredDataType { get; set; }
    }
}
