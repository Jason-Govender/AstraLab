namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Represents a request to open a previously stored analytics export for reading.
    /// </summary>
    public class OpenReadAnalyticsExportRequest
    {
        /// <summary>
        /// Gets or sets the storage provider that owns the export.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the export.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
