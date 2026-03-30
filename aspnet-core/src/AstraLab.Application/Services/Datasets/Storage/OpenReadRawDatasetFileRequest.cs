namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Represents a request to open a previously stored raw dataset file for reading.
    /// </summary>
    public class OpenReadRawDatasetFileRequest
    {
        /// <summary>
        /// Gets or sets the storage provider that owns the file.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the file.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
