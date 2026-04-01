using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Services.ML;
using AstraLab.Services.Storage;
using Microsoft.AspNetCore.WebUtilities;

namespace AstraLab.Web.Core.ML
{
    /// <summary>
    /// Creates and validates short-lived signed tokens for executor dataset download and artifact upload URLs.
    /// </summary>
    public class MLExecutorFileAccessTokenService : ITransientDependency
    {
        private readonly MLExecutionOptions _mlExecutionOptions;
        private readonly ObjectStorageOptions _objectStorageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExecutorFileAccessTokenService"/> class.
        /// </summary>
        public MLExecutorFileAccessTokenService(
            MLExecutionOptions mlExecutionOptions,
            ObjectStorageOptions objectStorageOptions)
        {
            _mlExecutionOptions = mlExecutionOptions;
            _objectStorageOptions = objectStorageOptions;
        }

        /// <summary>
        /// Creates a short-lived dataset download token.
        /// </summary>
        public string CreateDatasetDownloadToken(string storageProvider, string storageKey)
        {
            return Protect(new FileAccessTokenPayload
            {
                Purpose = "dataset-download",
                StorageProvider = storageProvider?.Trim(),
                StorageKey = storageKey?.Trim(),
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(_objectStorageOptions.PresignedUrlTtlSeconds)
            });
        }

        /// <summary>
        /// Creates a short-lived artifact upload token.
        /// </summary>
        public string CreateArtifactUploadToken(string storageProvider, string storageKey)
        {
            return Protect(new FileAccessTokenPayload
            {
                Purpose = "artifact-upload",
                StorageProvider = storageProvider?.Trim(),
                StorageKey = storageKey?.Trim(),
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(_objectStorageOptions.PresignedUrlTtlSeconds)
            });
        }

        /// <summary>
        /// Validates a dataset download token and returns its decoded payload.
        /// </summary>
        public FileAccessTokenPayload ValidateDatasetDownloadToken(string token)
        {
            return Validate(token, "dataset-download");
        }

        /// <summary>
        /// Validates an artifact upload token and returns its decoded payload.
        /// </summary>
        public FileAccessTokenPayload ValidateArtifactUploadToken(string token)
        {
            return Validate(token, "artifact-upload");
        }

        private FileAccessTokenPayload Validate(string token, string expectedPurpose)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UserFriendlyException("A file access token is required.");
            }

            var segments = token.Split('.');
            if (segments.Length != 2)
            {
                throw new UserFriendlyException("The supplied file access token is invalid.");
            }

            var payloadBytes = WebEncoders.Base64UrlDecode(segments[0]);
            var actualSignature = WebEncoders.Base64UrlDecode(segments[1]);
            var expectedSignature = ComputeSignature(payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
            {
                throw new UserFriendlyException("The supplied file access token signature is invalid.");
            }

            var payload = JsonSerializer.Deserialize<FileAccessTokenPayload>(payloadBytes);
            if (payload == null ||
                !string.Equals(payload.Purpose, expectedPurpose, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(payload.StorageProvider) ||
                string.IsNullOrWhiteSpace(payload.StorageKey))
            {
                throw new UserFriendlyException("The supplied file access token payload is invalid.");
            }

            if (payload.ExpiresAtUtc <= DateTime.UtcNow)
            {
                throw new UserFriendlyException("The supplied file access token has expired.");
            }

            return payload;
        }

        private string Protect(FileAccessTokenPayload payload)
        {
            var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            var signature = ComputeSignature(payloadBytes);
            return WebEncoders.Base64UrlEncode(payloadBytes) + "." + WebEncoders.Base64UrlEncode(signature);
        }

        private byte[] ComputeSignature(byte[] payloadBytes)
        {
            if (string.IsNullOrWhiteSpace(_mlExecutionOptions.SharedSecret))
            {
                throw new InvalidOperationException("The ML shared secret must be configured before file access tokens can be issued.");
            }

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_mlExecutionOptions.SharedSecret)))
            {
                return hmac.ComputeHash(payloadBytes);
            }
        }

        /// <summary>
        /// Represents the decoded information embedded in an executor file-access token.
        /// </summary>
        public class FileAccessTokenPayload
        {
            /// <summary>
            /// Gets or sets the intended token purpose.
            /// </summary>
            public string Purpose { get; set; }

            /// <summary>
            /// Gets or sets the logical storage provider name.
            /// </summary>
            public string StorageProvider { get; set; }

            /// <summary>
            /// Gets or sets the logical storage key.
            /// </summary>
            public string StorageKey { get; set; }

            /// <summary>
            /// Gets or sets the UTC expiry timestamp.
            /// </summary>
            public DateTime ExpiresAtUtc { get; set; }
        }
    }
}
