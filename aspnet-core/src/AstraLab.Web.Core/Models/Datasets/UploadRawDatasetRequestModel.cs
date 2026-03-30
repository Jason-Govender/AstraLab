using System.ComponentModel.DataAnnotations;
using AstraLab.Core.Domains.Datasets;
using Microsoft.AspNetCore.Http;

namespace AstraLab.Models.Datasets
{
    /// <summary>
    /// Represents the multipart form-data payload used to upload a raw dataset file.
    /// </summary>
    public class UploadRawDatasetRequestModel
    {
        /// <summary>
        /// Gets or sets the dataset display name.
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
        /// Gets or sets the uploaded raw dataset file.
        /// </summary>
        [Required]
        public IFormFile File { get; set; }
    }
}
