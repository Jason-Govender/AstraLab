namespace AstraLab.Services.ML
{
    /// <summary>
    /// Defines infrastructure settings used by the ML experiment dispatcher and callbacks.
    /// </summary>
    public class MLExecutionOptions
    {
        /// <summary>
        /// Gets or sets the base URL of the Python ML executor service.
        /// </summary>
        public string ExecutorBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the public callback base URL that the executor should call back into.
        /// </summary>
        public string CallbackBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the shared secret used for internal service-to-service authentication.
        /// </summary>
        public string SharedSecret { get; set; }

        /// <summary>
        /// Gets or sets the default provider used for new ML artifact writes.
        /// </summary>
        public string DefaultArtifactStorageProvider { get; set; } = "local-filesystem";

        /// <summary>
        /// Gets or sets the resolved local artifact root path used for legacy and development artifact files.
        /// </summary>
        public string ArtifactRootPath { get; set; }
    }
}
