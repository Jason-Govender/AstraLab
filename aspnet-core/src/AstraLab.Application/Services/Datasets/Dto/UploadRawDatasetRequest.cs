using System.ComponentModel.DataAnnotations;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the application-layer request used to upload a validated raw dataset file.
    /// </summary>
    public class UploadRawDatasetRequest
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
        /// Gets or sets the original uploaded file name.
        /// </summary>
        [Required]
        [StringLength(Dataset.MaxOriginalFileNameLength)]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the optional uploaded file content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the uploaded file bytes.
        /// </summary>
        [Required]
        public byte[] Content { get; set; }
    }
}
