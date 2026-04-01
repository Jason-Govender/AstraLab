namespace AstraLab.Services.ML.Storage
{
    /// <summary>
    /// Represents a concrete ML artifact storage provider implementation.
    /// </summary>
    public interface IMLArtifactStorageProvider : IMLArtifactStorage
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
