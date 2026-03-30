using System;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents lightweight raw file metadata for a dataset version.
    /// </summary>
    public class DatasetFileSummaryDto
    {
        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the uploaded file content type when known.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the stored raw file size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the raw file metadata record.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
