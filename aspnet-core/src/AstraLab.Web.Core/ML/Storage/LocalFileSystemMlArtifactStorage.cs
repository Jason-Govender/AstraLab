using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;

namespace AstraLab.Web.Core.ML.Storage
{
    /// <summary>
    /// Stores ML artifact files on the local filesystem for development and legacy deployments.
    /// </summary>
    public class LocalFileSystemMlArtifactStorage : IMLArtifactStorageProvider, ITransientDependency
    {
        /// <summary>
        /// Gets the persisted provider name for local filesystem ML artifact storage.
        /// </summary>
        public const string ProviderName = "local-filesystem";

        private readonly string _artifactRootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileSystemMlArtifactStorage"/> class.
        /// </summary>
        public LocalFileSystemMlArtifactStorage(MLExecutionOptions mlExecutionOptions)
        {
            if (mlExecutionOptions == null)
            {
                throw new ArgumentNullException(nameof(mlExecutionOptions));
            }

            if (mlExecutionOptions.ArtifactRootPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("A local ML artifact root path must be configured.", nameof(mlExecutionOptions));
            }

            _artifactRootPath = mlExecutionOptions.ArtifactRootPath;
        }

        /// <summary>
        /// Gets the persisted provider name handled by this implementation.
        /// </summary>
        public string ProviderNameValue => ProviderName;

        /// <summary>
        /// Gets a value indicating whether this provider can accept new writes.
        /// </summary>
        public bool CanStore => true;

        string IMLArtifactStorageProvider.ProviderName => ProviderNameValue;

        /// <summary>
        /// Stores an ML artifact at the specified logical storage key.
        /// </summary>
        public async Task<StoredMlArtifactResult> StoreAsync(StoreMlArtifactRequest request)
        {
            ValidateStoreRequest(request);

            var artifactPath = ResolveStoragePath(request.StorageKey);
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            var tempFilePath = Path.Combine(artifactDirectory, ".upload-" + Guid.NewGuid().ToString("N") + ".tmp");

            Directory.CreateDirectory(artifactDirectory);

            try
            {
                using (var output = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await request.Content.CopyToAsync(output);
                }

                if (File.Exists(artifactPath))
                {
                    File.Delete(artifactPath);
                }

                File.Move(tempFilePath, artifactPath);

                return new StoredMlArtifactResult
                {
                    StorageProvider = ProviderName,
                    StorageKey = request.StorageKey.Trim()
                };
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Opens a previously stored ML artifact for reading.
        /// </summary>
        public Task<Stream> OpenReadAsync(OpenReadMlArtifactRequest request)
        {
            ValidateOpenReadRequest(request);

            var artifactPath = ResolveStoragePath(request.StorageKey);
            if (!File.Exists(artifactPath))
            {
                throw new FileNotFoundException("The requested ML artifact file could not be found.", artifactPath);
            }

            Stream fileStream = new FileStream(artifactPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(fileStream);
        }

        /// <summary>
        /// Deletes a previously stored ML artifact.
        /// </summary>
        public Task DeleteAsync(DeleteMlArtifactRequest request)
        {
            ValidateDeleteRequest(request);

            var artifactPath = ResolveStoragePath(request.StorageKey);
            if (!File.Exists(artifactPath))
            {
                return Task.CompletedTask;
            }

            File.Delete(artifactPath);
            DeleteEmptyParentDirectories(artifactPath);
            return Task.CompletedTask;
        }

        private static void ValidateStoreRequest(StoreMlArtifactRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An artifact storage key is required.", nameof(request));
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new ArgumentException("A readable content stream is required.", nameof(request));
            }
        }

        private void ValidateOpenReadRequest(OpenReadMlArtifactRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the local ML artifact storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An artifact storage key is required.", nameof(request));
            }
        }

        private void ValidateDeleteRequest(DeleteMlArtifactRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the local ML artifact storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An artifact storage key is required.", nameof(request));
            }
        }

        private string ResolveStoragePath(string storageKey)
        {
            var normalizedStorageKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
            var fullRootPath = Path.GetFullPath(_artifactRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(fullRootPath, normalizedStorageKey));

            if (!fullPath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The artifact storage key resolves outside the configured artifact root.");
            }

            return fullPath;
        }

        private void DeleteEmptyParentDirectories(string filePath)
        {
            var fullRootPath = Path.GetFullPath(_artifactRootPath).TrimEnd(Path.DirectorySeparatorChar);
            var directory = new DirectoryInfo(Path.GetDirectoryName(filePath));

            while (directory != null &&
                   !string.Equals(directory.FullName.TrimEnd(Path.DirectorySeparatorChar), fullRootPath, StringComparison.OrdinalIgnoreCase) &&
                   !directory.EnumerateFileSystemInfos().Any())
            {
                var parentDirectory = directory.Parent;
                directory.Delete();
                directory = parentDirectory;
            }
        }
    }
}
