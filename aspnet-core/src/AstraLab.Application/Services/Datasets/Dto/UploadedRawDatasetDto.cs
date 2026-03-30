using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the result of a successful raw dataset upload.
    /// </summary>
    public class UploadedRawDatasetDto
    {
        /// <summary>
        /// Gets or sets the created dataset metadata.
        /// </summary>
        public DatasetDto Dataset { get; set; }

        /// <summary>
        /// Gets or sets the created dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the storage provider used for the raw file.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the raw file.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the stored raw file size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the SHA-256 checksum of the stored raw file.
        /// </summary>
        public string ChecksumSha256 { get; set; }

        /// <summary>
        /// Gets or sets the extracted column count for the uploaded dataset version.
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized extracted schema payload.
        /// </summary>
        public string SchemaJson { get; set; }

        /// <summary>
        /// Gets or sets the extracted columns returned from the upload workflow.
        /// </summary>
        public List<DatasetColumnDto> Columns { get; set; } = new List<DatasetColumnDto>();
    }
}
