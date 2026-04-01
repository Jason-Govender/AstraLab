namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Defines configuration values for raw dataset storage.
    /// </summary>
    public class DatasetStorageOptions
    {
        /// <summary>
        /// Gets or sets the provider used for new dataset writes.
        /// </summary>
        public string DefaultProvider { get; set; } = "local-filesystem";

        /// <summary>
        /// Gets or sets the resolved root directory used to store immutable raw dataset files.
        /// </summary>
        public string RawRootPath { get; set; }
    }
}
