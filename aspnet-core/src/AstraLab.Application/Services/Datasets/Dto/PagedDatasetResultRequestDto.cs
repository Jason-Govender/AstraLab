using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the paging and filter options for dataset queries.
    /// </summary>
    public class PagedDatasetResultRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the optional keyword filter applied to name and description.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// Gets or sets the optional status filter.
        /// </summary>
        public DatasetStatus? Status { get; set; }
    }
}
