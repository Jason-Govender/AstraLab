namespace AstraLab.Services.Analytics.Storage
{
    /// <summary>
    /// Represents a concrete analytics export storage provider implementation.
    /// </summary>
    public interface IAnalyticsExportStorageProvider : IAnalyticsExportStorage
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
