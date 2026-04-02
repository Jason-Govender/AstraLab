using System.IO;

namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Carries the data needed to store an analytics export file.
    /// </summary>
    public class StoreAnalyticsExportRequest
    {
        /// <summary>
        /// Gets or sets the optional provider to target. When omitted, the configured default provider is used.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the export.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the readable content stream for the export.
        /// </summary>
        public Stream Content { get; set; }
    }
}
