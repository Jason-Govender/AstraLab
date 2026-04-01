namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Represents the persisted logical reference returned after an ML artifact is stored.
    /// </summary>
    public class StoredMlArtifactResult
    {
        /// <summary>
        /// Gets or sets the storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical artifact storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
