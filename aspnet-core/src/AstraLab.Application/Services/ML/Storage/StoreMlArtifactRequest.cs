using System.IO;

namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Carries the data needed to store an ML artifact file.
    /// </summary>
    public class StoreMlArtifactRequest
    {
        /// <summary>
        /// Gets or sets the optional provider to target. When omitted, the configured default provider is used.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the artifact.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the readable content stream for the artifact.
        /// </summary>
        public Stream Content { get; set; }
    }
}
