namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Represents a concrete dataset storage provider implementation.
    /// </summary>
    public interface IRawDatasetStorageProvider : IRawDatasetStorage
    {
        /// <summary>
        /// Gets the persisted provider name handled by this implementation.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Gets a value indicating whether this provider can accept new writes.
        /// </summary>
        bool CanStore { get; }
    }
}
