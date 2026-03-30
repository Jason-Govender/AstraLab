using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents paging and filter options for dataset version queries.
    /// </summary>
    public class PagedDatasetVersionResultRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset identifier that owns the versions.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the optional version status filter.
        /// </summary>
        public DatasetVersionStatus? Status { get; set; }
    }
}
