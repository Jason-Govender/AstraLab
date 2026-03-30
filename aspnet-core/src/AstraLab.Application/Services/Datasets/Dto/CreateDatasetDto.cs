using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the metadata required to register a dataset.
    /// </summary>
    [AutoMapTo(typeof(Dataset))]
    public class CreateDatasetDto
    {
        /// <summary>
        /// Gets or sets the dataset name.
        /// </summary>
        [Required]
        [StringLength(Dataset.MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset description.
        /// </summary>
        [StringLength(Dataset.MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the dataset source format.
        /// </summary>
        [Required]
        public DatasetFormat SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        [Required]
        [StringLength(Dataset.MaxOriginalFileNameLength)]
        public string OriginalFileName { get; set; }
    }
}
