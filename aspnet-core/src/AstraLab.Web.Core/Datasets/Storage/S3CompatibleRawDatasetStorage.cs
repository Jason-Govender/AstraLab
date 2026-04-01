using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Amazon.S3;
using Amazon.S3.Model;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.Storage;
using AstraLab.Web.Core.Storage;

namespace AstraLab.Web.Core.Datasets.Storage
{
    /// <summary>
    /// Stores immutable dataset version files in an S3-compatible object store.
    /// </summary>
    public class S3CompatibleRawDatasetStorage : IRawDatasetStorageProvider, ITransientDependency
    {
        /// <summary>
        /// Gets the persisted provider name for S3-compatible dataset storage.
        /// </summary>
        public const string ProviderName = "s3-compatible";

        private readonly ObjectStorageOptions _objectStorageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3CompatibleRawDatasetStorage"/> class.
        /// </summary>
        public S3CompatibleRawDatasetStorage(ObjectStorageOptions objectStorageOptions)
        {
            _objectStorageOptions = objectStorageOptions;
        }

        /// <summary>
        /// Gets the persisted provider name handled by this implementation.
        /// </summary>
        public string ProviderNameValue => ProviderName;

        /// <summary>
        /// Gets a value indicating whether this provider can accept new writes.
        /// </summary>
        public bool CanStore => true;

        string IRawDatasetStorageProvider.ProviderName => ProviderNameValue;

        /// <summary>
        /// Stores the supplied dataset file in S3-compatible object storage.
        /// </summary>
        public async Task<StoredRawDatasetFileResult> StoreAsync(StoreRawDatasetFileRequest request)
        {
            ValidateRequest(request);
            ValidateBucketConfiguration();

            var originalFileName = Path.GetFileName(request.OriginalFileName.Trim());
            var tempFilePath = Path.Combine(Path.GetTempPath(), "AstraLab", "DatasetStorage", Guid.NewGuid().ToString("N") + ".tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

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

                var storageKey = DatasetStorageKeyBuilder.BuildStorageKey(request, checksumSha256);
                var objectKey = S3CompatibleStoragePathBuilder.BuildDatasetObjectKey(_objectStorageOptions, storageKey);

                using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
                {
                    if (await ObjectExistsAsync(client, objectKey))
                    {
                        throw new IOException("An immutable dataset object already exists for the requested logical key.");
                    }

                    using (var readStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await client.PutObjectAsync(new PutObjectRequest
                        {
                            BucketName = _objectStorageOptions.DatasetBucketName,
                            Key = objectKey,
                            InputStream = readStream,
                            AutoCloseStream = false,
                            // Cloudflare R2 requires the AWS .NET SDK streaming upload integrity
                            // features to be disabled for PutObject requests.
                            DisablePayloadSigning = true,
                            DisableDefaultChecksumValidation = true
                        });
                    }
                }

                return new StoredRawDatasetFileResult
                {
                    StorageProvider = ProviderName,
                    StorageKey = storageKey,
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
        /// Opens a previously stored dataset file for read access.
        /// </summary>
        public async Task<Stream> OpenReadAsync(OpenReadRawDatasetFileRequest request)
        {
            ValidateOpenReadRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                var response = await client.GetObjectAsync(_objectStorageOptions.DatasetBucketName, S3CompatibleStoragePathBuilder.BuildDatasetObjectKey(_objectStorageOptions, request.StorageKey));
                var memoryStream = new MemoryStream();
                using (response)
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);
                }

                memoryStream.Position = 0;
                return memoryStream;
            }
        }

        /// <summary>
        /// Deletes a previously stored dataset file.
        /// </summary>
        public async Task DeleteAsync(DeleteRawDatasetFileRequest request)
        {
            ValidateDeleteRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                await client.DeleteObjectAsync(_objectStorageOptions.DatasetBucketName, S3CompatibleStoragePathBuilder.BuildDatasetObjectKey(_objectStorageOptions, request.StorageKey));
            }
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

        private void ValidateOpenReadRequest(OpenReadRawDatasetFileRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible dataset storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The storage key is required.", nameof(request));
            }
        }

        private void ValidateDeleteRequest(DeleteRawDatasetFileRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible dataset storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The storage key is required.", nameof(request));
            }
        }

        private void ValidateBucketConfiguration()
        {
            S3CompatibleObjectStorageOptionsValidator.ValidateBaseConfiguration(_objectStorageOptions);

            if (string.IsNullOrWhiteSpace(_objectStorageOptions.DatasetBucketName))
            {
                throw new InvalidOperationException("The dataset object-storage bucket is not configured.");
            }
        }

        private async Task<bool> ObjectExistsAsync(IAmazonS3 client, string objectKey)
        {
            try
            {
                await client.GetObjectMetadataAsync(_objectStorageOptions.DatasetBucketName, objectKey);
                return true;
            }
            catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
