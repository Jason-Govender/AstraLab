using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using AstraLab.Services.Datasets.Storage;

namespace AstraLab.Web.Core.Datasets.Storage
{
    /// <summary>
    /// Routes dataset storage operations to the configured concrete provider implementations.
    /// </summary>
    public class CompositeRawDatasetStorage : IRawDatasetStorage, ITransientDependency
    {
        private readonly DatasetStorageOptions _datasetStorageOptions;
        private readonly IReadOnlyDictionary<string, IRawDatasetStorageProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeRawDatasetStorage"/> class.
        /// </summary>
        public CompositeRawDatasetStorage(
            DatasetStorageOptions datasetStorageOptions,
            IEnumerable<IRawDatasetStorageProvider> providers)
        {
            if (datasetStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(datasetStorageOptions));
            }

            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _datasetStorageOptions = datasetStorageOptions;
            _providers = providers.ToDictionary(item => item.ProviderName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Stores the supplied dataset file using the configured default write provider.
        /// </summary>
        public Task<StoredRawDatasetFileResult> StoreAsync(StoreRawDatasetFileRequest request)
        {
            return ResolveDefaultWriteProvider().StoreAsync(request);
        }

        /// <summary>
        /// Opens a previously stored dataset file by its persisted logical reference.
        /// </summary>
        public Task<System.IO.Stream> OpenReadAsync(OpenReadRawDatasetFileRequest request)
        {
            return ResolveProvider(request.StorageProvider).OpenReadAsync(request);
        }

        /// <summary>
        /// Deletes a previously stored dataset file by its persisted logical reference.
        /// </summary>
        public Task DeleteAsync(DeleteRawDatasetFileRequest request)
        {
            return ResolveProvider(request.StorageProvider).DeleteAsync(request);
        }

        private IRawDatasetStorageProvider ResolveDefaultWriteProvider()
        {
            var providerName = string.IsNullOrWhiteSpace(_datasetStorageOptions.DefaultProvider)
                ? LocalFileSystemRawDatasetStorage.ProviderName
                : _datasetStorageOptions.DefaultProvider.Trim();

            var provider = ResolveProvider(providerName);
            if (!provider.CanStore)
            {
                throw new InvalidOperationException(string.Format("The dataset storage provider '{0}' cannot accept new writes.", providerName));
            }

            return provider;
        }

        private IRawDatasetStorageProvider ResolveProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("A dataset storage provider name is required.", nameof(providerName));
            }

            if (_providers.TryGetValue(providerName.Trim(), out var provider))
            {
                return provider;
            }

            throw new InvalidOperationException(string.Format("The dataset storage provider '{0}' is not registered.", providerName));
        }
    }
}
