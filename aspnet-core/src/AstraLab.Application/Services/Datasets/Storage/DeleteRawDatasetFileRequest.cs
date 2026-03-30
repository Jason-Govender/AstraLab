namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Carries the logical reference needed to delete a stored raw dataset file.
    /// </summary>
    public class DeleteRawDatasetFileRequest
    {
        /// <summary>
        /// Gets or sets the persisted storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted logical storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
