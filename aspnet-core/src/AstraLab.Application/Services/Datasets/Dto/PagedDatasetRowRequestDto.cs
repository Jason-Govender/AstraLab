using Abp.Application.Services.Dto;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a paged request for dataset-version rows.
    /// </summary>
    public class PagedDatasetRowRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the requested rows.
        /// </summary>
        public long DatasetVersionId { get; set; }
    }
}
