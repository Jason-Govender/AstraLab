namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Represents the persisted logical reference returned after an analytics export is stored.
    /// </summary>
    public class StoredAnalyticsExportResult
    {
        /// <summary>
        /// Gets or sets the storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical analytics export storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
