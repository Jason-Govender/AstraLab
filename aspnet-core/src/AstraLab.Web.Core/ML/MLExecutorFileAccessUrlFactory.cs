using System;
using Abp.Dependency;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;
using AstraLab.Web.Core.ML.Storage;

namespace AstraLab.Web.Core.ML
{
    /// <summary>
    /// Builds short-lived backend URLs that the ML executor uses to download datasets and upload artifacts.
    /// </summary>
    public class MLExecutorFileAccessUrlFactory : ITransientDependency
    {
        private readonly MLExecutionOptions _mlExecutionOptions;
        private readonly MLExecutorFileAccessTokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExecutorFileAccessUrlFactory"/> class.
        /// </summary>
        public MLExecutorFileAccessUrlFactory(
            MLExecutionOptions mlExecutionOptions,
            MLExecutorFileAccessTokenService tokenService)
        {
            _mlExecutionOptions = mlExecutionOptions;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Creates a short-lived dataset download URL for the supplied logical storage reference.
        /// </summary>
        public string CreateDatasetDownloadUrl(string storageProvider, string storageKey)
        {
            return string.Format(
                "{0}/api/internal/ml-storage/datasets?token={1}",
                ResolveBackendBaseUrl(),
                _tokenService.CreateDatasetDownloadToken(storageProvider, storageKey));
        }

        /// <summary>
        /// Creates a short-lived artifact upload target for the supplied experiment.
        /// </summary>
        public MlArtifactUploadTarget CreateArtifactUploadTarget(int tenantId, long experimentId)
        {
            var storageProvider = string.IsNullOrWhiteSpace(_mlExecutionOptions.DefaultArtifactStorageProvider)
                ? LocalFileSystemMlArtifactStorage.ProviderName
                : _mlExecutionOptions.DefaultArtifactStorageProvider.Trim();
            var storageKey = MlArtifactStorageKeyBuilder.BuildStorageKey(tenantId, experimentId);

            return new MlArtifactUploadTarget
            {
                StorageProvider = storageProvider,
                StorageKey = storageKey,
                UploadUrl = string.Format(
                    "{0}/api/internal/ml-storage/artifacts?token={1}",
                    ResolveBackendBaseUrl(),
                    _tokenService.CreateArtifactUploadToken(storageProvider, storageKey))
            };
        }

        private string ResolveBackendBaseUrl()
        {
            if (string.IsNullOrWhiteSpace(_mlExecutionOptions.CallbackBaseUrl))
            {
                throw new InvalidOperationException("The ML callback base URL must be configured before executor file access URLs can be issued.");
            }

            return _mlExecutionOptions.CallbackBaseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Represents the generated artifact upload target sent to the ML executor.
        /// </summary>
        public class MlArtifactUploadTarget
        {
            /// <summary>
            /// Gets or sets the logical artifact storage provider.
            /// </summary>
            public string StorageProvider { get; set; }

            /// <summary>
            /// Gets or sets the logical artifact storage key.
            /// </summary>
            public string StorageKey { get; set; }

            /// <summary>
            /// Gets or sets the temporary backend upload URL.
            /// </summary>
            public string UploadUrl { get; set; }
        }
    }
}
