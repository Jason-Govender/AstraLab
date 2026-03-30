namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Represents the immutable storage reference returned after a raw dataset file is stored.
    /// </summary>
    public class StoredRawDatasetFileResult
    {
        /// <summary>
        /// Gets or sets the storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the stored file size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the SHA-256 checksum of the stored file.
        /// </summary>
        public string ChecksumSha256 { get; set; }
    }
}
