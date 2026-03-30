using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using AstraLab.Services.Datasets.Storage;

namespace AstraLab.Web.Core.Datasets.Storage
{
    /// <summary>
    /// Stores immutable raw dataset files on the local filesystem.
    /// </summary>
    public class LocalFileSystemRawDatasetStorage : IRawDatasetStorage, ITransientDependency
    {
        /// <summary>
        /// Gets the persisted provider name for local filesystem storage.
        /// </summary>
        public const string ProviderName = "local-filesystem";

        private readonly string _rawRootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileSystemRawDatasetStorage"/> class.
        /// </summary>
        public LocalFileSystemRawDatasetStorage(DatasetStorageOptions datasetStorageOptions)
        {
            if (datasetStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(datasetStorageOptions));
            }

            if (datasetStorageOptions.RawRootPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Raw dataset storage root path must be configured.", nameof(datasetStorageOptions));
            }

            _rawRootPath = datasetStorageOptions.RawRootPath;
        }

        /// <summary>
        /// Stores the supplied raw dataset content in immutable local storage and returns a logical reference.
        /// </summary>
        public async Task<StoredRawDatasetFileResult> StoreAsync(StoreRawDatasetFileRequest request)
        {
            ValidateRequest(request);

            var originalFileName = Path.GetFileName(request.OriginalFileName.Trim());
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var storageDirectory = BuildStorageDirectory(request);
            var tempFilePath = Path.Combine(storageDirectory, $".upload-{Guid.NewGuid():N}.tmp");

            Directory.CreateDirectory(storageDirectory);

            try
            {
                string checksumSha256;
                long sizeBytes;

                using (var tempFileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var sha256 = SHA256.Create())
                using (var cryptoStream = new CryptoStream(tempFileStream, sha256, CryptoStreamMode.Write))
                {
                    await request.Content.CopyToAsync(cryptoStream);
                    cryptoStream.FlushFinalBlock();

                    checksumSha256 = Convert.ToHexString(sha256.Hash).ToLowerInvariant();
                    sizeBytes = tempFileStream.Length;
                }

                var finalFileName = $"{checksumSha256}{extension}";
                var finalFilePath = Path.Combine(storageDirectory, finalFileName);

                File.Move(tempFilePath, finalFilePath, false);

                return new StoredRawDatasetFileResult
                {
                    StorageProvider = ProviderName,
                    StorageKey = BuildStorageKey(request, finalFileName),
                    OriginalFileName = originalFileName,
                    SizeBytes = sizeBytes,
                    ChecksumSha256 = checksumSha256
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
        /// Deletes a previously stored raw dataset file by logical reference.
        /// </summary>
        public Task DeleteAsync(DeleteRawDatasetFileRequest request)
        {
            ValidateDeleteRequest(request);

            var filePath = ResolveStoragePath(request.StorageKey);
            if (!File.Exists(filePath))
            {
                return Task.CompletedTask;
            }

            File.Delete(filePath);
            DeleteEmptyParentDirectories(filePath);
            return Task.CompletedTask;
        }

        private static void ValidateRequest(StoreRawDatasetFileRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.TenantId <= 0)
            {
                throw new ArgumentException("A valid tenant identifier is required.", nameof(request));
            }

            if (request.DatasetId <= 0)
            {
                throw new ArgumentException("A valid dataset identifier is required.", nameof(request));
            }

            if (request.DatasetVersionId <= 0)
            {
                throw new ArgumentException("A valid dataset version identifier is required.", nameof(request));
            }

            if (request.OriginalFileName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The original file name is required.", nameof(request));
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new ArgumentException("A readable content stream is required.", nameof(request));
            }
        }

        private string BuildStorageDirectory(StoreRawDatasetFileRequest request)
        {
            return Path.Combine(
                _rawRootPath,
                "tenants",
                request.TenantId.ToString(),
                "datasets",
                request.DatasetId.ToString(),
                "versions",
                request.DatasetVersionId.ToString(),
                "raw");
        }

        private static string BuildStorageKey(StoreRawDatasetFileRequest request, string finalFileName)
        {
            return $"tenants/{request.TenantId}/datasets/{request.DatasetId}/versions/{request.DatasetVersionId}/raw/{finalFileName}";
        }

        private void ValidateDeleteRequest(DeleteRawDatasetFileRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the local filesystem storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The storage key is required.", nameof(request));
            }
        }

        private string ResolveStoragePath(string storageKey)
        {
            var normalizedStorageKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
            var fullRootPath = Path.GetFullPath(_rawRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(fullRootPath, normalizedStorageKey));

            if (!fullPath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The storage key resolves outside the configured raw dataset storage root.");
            }

            return fullPath;
        }

        private void DeleteEmptyParentDirectories(string filePath)
        {
            var fullRootPath = Path.GetFullPath(_rawRootPath).TrimEnd(Path.DirectorySeparatorChar);
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
