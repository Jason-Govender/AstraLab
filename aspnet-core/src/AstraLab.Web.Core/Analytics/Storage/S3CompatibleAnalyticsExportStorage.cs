using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using AstraLab.Services.Analytics.Storage;
using AstraLab.Services.Storage;
using AstraLab.Web.Core.Storage;

namespace AstraLab.Web.Core.Analytics.Storage
{
    /// <summary>
    /// Stores analytics exports in an S3-compatible object store.
    /// </summary>
    public class S3CompatibleAnalyticsExportStorage : IAnalyticsExportStorageProvider, ITransientDependency
    {
        /// <summary>
        /// Gets the persisted provider name for S3-compatible analytics export storage.
        /// </summary>
        public const string ProviderName = "s3-compatible";

        private readonly ObjectStorageOptions _objectStorageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3CompatibleAnalyticsExportStorage"/> class.
        /// </summary>
        public S3CompatibleAnalyticsExportStorage(ObjectStorageOptions objectStorageOptions)
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

        string IAnalyticsExportStorageProvider.ProviderName => ProviderNameValue;

        /// <summary>
        /// Stores an analytics export at the specified logical storage key.
        /// </summary>
        public async Task<StoredAnalyticsExportResult> StoreAsync(StoreAnalyticsExportRequest request)
        {
            ValidateStoreRequest(request);
            ValidateBucketConfiguration();

            using (var uploadStream = await CreateSeekableUploadStreamAsync(request.Content))
            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                await client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _objectStorageOptions.AnalyticsExportBucketName,
                    Key = S3CompatibleStoragePathBuilder.BuildAnalyticsExportObjectKey(_objectStorageOptions, request.StorageKey),
                    InputStream = uploadStream,
                    AutoCloseStream = false,
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true
                });
            }

            return new StoredAnalyticsExportResult
            {
                StorageProvider = ProviderName,
                StorageKey = request.StorageKey.Trim()
            };
        }

        /// <summary>
        /// Opens a previously stored analytics export for reading.
        /// </summary>
        public async Task<Stream> OpenReadAsync(OpenReadAnalyticsExportRequest request)
        {
            ValidateOpenReadRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                var response = await client.GetObjectAsync(_objectStorageOptions.AnalyticsExportBucketName, S3CompatibleStoragePathBuilder.BuildAnalyticsExportObjectKey(_objectStorageOptions, request.StorageKey));
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
        /// Deletes a previously stored analytics export.
        /// </summary>
        public async Task DeleteAsync(DeleteAnalyticsExportRequest request)
        {
            ValidateDeleteRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                await client.DeleteObjectAsync(_objectStorageOptions.AnalyticsExportBucketName, S3CompatibleStoragePathBuilder.BuildAnalyticsExportObjectKey(_objectStorageOptions, request.StorageKey));
            }
        }

        private static async Task<Stream> CreateSeekableUploadStreamAsync(Stream content)
        {
            if (content.CanSeek)
            {
                content.Position = 0;
                return content;
            }

            var bufferedStream = new MemoryStream();
            await content.CopyToAsync(bufferedStream);
            bufferedStream.Position = 0;
            return bufferedStream;
        }

        private static void ValidateStoreRequest(StoreAnalyticsExportRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An analytics export storage key is required.", nameof(request));
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new ArgumentException("A readable content stream is required.", nameof(request));
            }
        }

        private static void ValidateOpenReadRequest(OpenReadAnalyticsExportRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible analytics export storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An analytics export storage key is required.", nameof(request));
            }
        }

        private static void ValidateDeleteRequest(DeleteAnalyticsExportRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.Equals(request.StorageProvider, ProviderName, StringComparison.Ordinal))
            {
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible analytics export storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An analytics export storage key is required.", nameof(request));
            }
        }

        private void ValidateBucketConfiguration()
        {
            S3CompatibleObjectStorageOptionsValidator.ValidateBaseConfiguration(_objectStorageOptions);

            if (string.IsNullOrWhiteSpace(_objectStorageOptions.AnalyticsExportBucketName))
            {
                throw new InvalidOperationException("The analytics export object-storage bucket is not configured.");
            }
        }
    }
}
