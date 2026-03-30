using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents paging options for dataset column queries.
    /// </summary>
    public class PagedDatasetColumnResultRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the columns.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long DatasetVersionId { get; set; }
    }
}
