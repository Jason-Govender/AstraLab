using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;

namespace AstraLab.Web.Core.ML.Storage
{
    /// <summary>
    /// Routes ML artifact storage operations to the configured concrete provider implementations.
    /// </summary>
    public class CompositeMlArtifactStorage : IMLArtifactStorage, ITransientDependency
    {
        private readonly MLExecutionOptions _mlExecutionOptions;
        private readonly IReadOnlyDictionary<string, IMLArtifactStorageProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMlArtifactStorage"/> class.
        /// </summary>
        public CompositeMlArtifactStorage(
            MLExecutionOptions mlExecutionOptions,
            IEnumerable<IMLArtifactStorageProvider> providers)
        {
            if (mlExecutionOptions == null)
            {
                throw new ArgumentNullException(nameof(mlExecutionOptions));
            }

            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _mlExecutionOptions = mlExecutionOptions;
            _providers = providers.ToDictionary(item => item.ProviderName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Stores the supplied artifact using the requested or configured default provider.
        /// </summary>
        public Task<StoredMlArtifactResult> StoreAsync(StoreMlArtifactRequest request)
        {
            return ResolveWriteProvider(request).StoreAsync(request);
        }

        /// <summary>
        /// Opens a previously stored artifact by its persisted logical reference.
        /// </summary>
        public Task<System.IO.Stream> OpenReadAsync(OpenReadMlArtifactRequest request)
        {
            return ResolveProvider(request.StorageProvider).OpenReadAsync(request);
        }

        /// <summary>
        /// Deletes a previously stored artifact by its persisted logical reference.
        /// </summary>
        public Task DeleteAsync(DeleteMlArtifactRequest request)
        {
            return ResolveProvider(request.StorageProvider).DeleteAsync(request);
        }

        private IMLArtifactStorageProvider ResolveWriteProvider(StoreMlArtifactRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var providerName = string.IsNullOrWhiteSpace(request.StorageProvider)
                ? _mlExecutionOptions.DefaultArtifactStorageProvider
                : request.StorageProvider;

            var provider = ResolveProvider(providerName);
            if (!provider.CanStore)
            {
                throw new InvalidOperationException(string.Format("The ML artifact storage provider '{0}' cannot accept new writes.", providerName));
            }

            return provider;
        }

        private IMLArtifactStorageProvider ResolveProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("An ML artifact storage provider name is required.", nameof(providerName));
            }

            if (_providers.TryGetValue(providerName.Trim(), out var provider))
            {
                return provider;
            }

            throw new InvalidOperationException(string.Format("The ML artifact storage provider '{0}' is not registered.", providerName));
        }
    }
}
