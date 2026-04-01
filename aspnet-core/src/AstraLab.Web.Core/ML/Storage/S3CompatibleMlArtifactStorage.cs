using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using AstraLab.Services.ML.Storage;
using AstraLab.Services.Storage;
using AstraLab.Web.Core.Storage;

namespace AstraLab.Web.Core.ML.Storage
{
    /// <summary>
    /// Stores ML artifacts in an S3-compatible object store.
    /// </summary>
    public class S3CompatibleMlArtifactStorage : IMLArtifactStorageProvider, ITransientDependency
    {
        /// <summary>
        /// Gets the persisted provider name for S3-compatible ML artifact storage.
        /// </summary>
        public const string ProviderName = "s3-compatible";

        private readonly ObjectStorageOptions _objectStorageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3CompatibleMlArtifactStorage"/> class.
        /// </summary>
        public S3CompatibleMlArtifactStorage(ObjectStorageOptions objectStorageOptions)
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

        string IMLArtifactStorageProvider.ProviderName => ProviderNameValue;

        /// <summary>
        /// Stores an ML artifact at the specified logical storage key.
        /// </summary>
        public async Task<StoredMlArtifactResult> StoreAsync(StoreMlArtifactRequest request)
        {
            ValidateStoreRequest(request);
            ValidateBucketConfiguration();

            using (var uploadStream = await CreateSeekableUploadStreamAsync(request.Content))
            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                await client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _objectStorageOptions.MlArtifactBucketName,
                    Key = S3CompatibleStoragePathBuilder.BuildMlArtifactObjectKey(_objectStorageOptions, request.StorageKey),
                    InputStream = uploadStream,
                    AutoCloseStream = false,
                    // Cloudflare R2 requires the AWS .NET SDK streaming upload integrity
                    // features to be disabled for PutObject requests.
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true
                });
            }

            return new StoredMlArtifactResult
            {
                StorageProvider = ProviderName,
                StorageKey = request.StorageKey.Trim()
            };
        }

        /// <summary>
        /// Opens a previously stored ML artifact for reading.
        /// </summary>
        public async Task<Stream> OpenReadAsync(OpenReadMlArtifactRequest request)
        {
            ValidateOpenReadRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                var response = await client.GetObjectAsync(_objectStorageOptions.MlArtifactBucketName, S3CompatibleStoragePathBuilder.BuildMlArtifactObjectKey(_objectStorageOptions, request.StorageKey));
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
        /// Deletes a previously stored ML artifact.
        /// </summary>
        public async Task DeleteAsync(DeleteMlArtifactRequest request)
        {
            ValidateDeleteRequest(request);
            ValidateBucketConfiguration();

            using (var client = S3CompatibleStorageClientFactory.Create(_objectStorageOptions))
            {
                await client.DeleteObjectAsync(_objectStorageOptions.MlArtifactBucketName, S3CompatibleStoragePathBuilder.BuildMlArtifactObjectKey(_objectStorageOptions, request.StorageKey));
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
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible ML artifact storage.", nameof(request));
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
                throw new ArgumentException("The specified storage provider is not supported by the S3-compatible ML artifact storage.", nameof(request));
            }

            if (request.StorageKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("An artifact storage key is required.", nameof(request));
            }
        }

        private void ValidateBucketConfiguration()
        {
            S3CompatibleObjectStorageOptionsValidator.ValidateBaseConfiguration(_objectStorageOptions);

            if (string.IsNullOrWhiteSpace(_objectStorageOptions.MlArtifactBucketName))
            {
                throw new InvalidOperationException("The ML artifact object-storage bucket is not configured.");
            }
        }
    }
}
