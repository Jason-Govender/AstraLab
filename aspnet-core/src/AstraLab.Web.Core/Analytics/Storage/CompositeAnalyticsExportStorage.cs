using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using AstraLab.Services.Analytics.Storage;

namespace AstraLab.Web.Core.Analytics.Storage
{
    /// <summary>
    /// Routes analytics export storage operations to the configured concrete provider implementations.
    /// </summary>
    public class CompositeAnalyticsExportStorage : IAnalyticsExportStorage, ITransientDependency
    {
        private const string DEFAULT_PROVIDER_NAME = S3CompatibleAnalyticsExportStorage.ProviderName;

        private readonly IReadOnlyDictionary<string, IAnalyticsExportStorageProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeAnalyticsExportStorage"/> class.
        /// </summary>
        public CompositeAnalyticsExportStorage(IEnumerable<IAnalyticsExportStorageProvider> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _providers = providers.ToDictionary(item => item.ProviderName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Stores the supplied export using the requested or default provider.
        /// </summary>
        public Task<StoredAnalyticsExportResult> StoreAsync(StoreAnalyticsExportRequest request)
        {
            return ResolveWriteProvider(request).StoreAsync(request);
        }

        /// <summary>
        /// Opens a previously stored analytics export by its persisted logical reference.
        /// </summary>
        public Task<System.IO.Stream> OpenReadAsync(OpenReadAnalyticsExportRequest request)
        {
            return ResolveProvider(request.StorageProvider).OpenReadAsync(request);
        }

        /// <summary>
        /// Deletes a previously stored analytics export by its persisted logical reference.
        /// </summary>
        public Task DeleteAsync(DeleteAnalyticsExportRequest request)
        {
            return ResolveProvider(request.StorageProvider).DeleteAsync(request);
        }

        private IAnalyticsExportStorageProvider ResolveWriteProvider(StoreAnalyticsExportRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var providerName = string.IsNullOrWhiteSpace(request.StorageProvider)
                ? DEFAULT_PROVIDER_NAME
                : request.StorageProvider;

            var provider = ResolveProvider(providerName);
            if (!provider.CanStore)
            {
                throw new InvalidOperationException(string.Format("The analytics export storage provider '{0}' cannot accept new writes.", providerName));
            }

            return provider;
        }

        private IAnalyticsExportStorageProvider ResolveProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("An analytics export storage provider name is required.", nameof(providerName));
            }

            if (_providers.TryGetValue(providerName.Trim(), out var provider))
            {
                return provider;
            }

            throw new InvalidOperationException(string.Format("The analytics export storage provider '{0}' is not registered.", providerName));
        }
    }
}
