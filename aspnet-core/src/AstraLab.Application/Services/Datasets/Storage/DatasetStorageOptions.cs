namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Defines configuration values for raw dataset storage.
    /// </summary>
    public class DatasetStorageOptions
    {
        /// <summary>
        /// Gets or sets the resolved root directory used to store immutable raw dataset files.
        /// </summary>
        public string RawRootPath { get; set; }
    }
}
