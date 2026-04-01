namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Represents a request to open a previously stored ML artifact for reading.
    /// </summary>
    public class OpenReadMlArtifactRequest
    {
        /// <summary>
        /// Gets or sets the storage provider that owns the artifact.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the artifact.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
