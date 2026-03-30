using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a full replacement request for dataset version columns.
    /// </summary>
    public class ReplaceDatasetColumnsDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the columns.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the replacement column set for the dataset version.
        /// </summary>
        public List<ReplaceDatasetColumnItemDto> Columns { get; set; } = new List<ReplaceDatasetColumnItemDto>();
    }
}
