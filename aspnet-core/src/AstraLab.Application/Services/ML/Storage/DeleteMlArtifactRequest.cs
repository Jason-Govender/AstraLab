namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Carries the logical reference needed to delete a stored ML artifact.
    /// </summary>
    public class DeleteMlArtifactRequest
    {
        /// <summary>
        /// Gets or sets the persisted storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted logical artifact storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
