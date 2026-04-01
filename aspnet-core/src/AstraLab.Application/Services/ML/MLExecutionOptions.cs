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
        /// Gets or sets the resolved artifact root path used by the executor.
        /// </summary>
        public string ArtifactRootPath { get; set; }
    }
}
