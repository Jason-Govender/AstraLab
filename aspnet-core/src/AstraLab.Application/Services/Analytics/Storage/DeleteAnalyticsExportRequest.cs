namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Carries the logical reference needed to delete a stored analytics export.
    /// </summary>
    public class DeleteAnalyticsExportRequest
    {
        /// <summary>
        /// Gets or sets the persisted storage provider name.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the persisted logical analytics export storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
