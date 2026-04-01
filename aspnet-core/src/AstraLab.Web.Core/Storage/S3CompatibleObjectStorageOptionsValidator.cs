using System;
using AstraLab.Services.Storage;

namespace AstraLab.Web.Core.Storage
{
    /// <summary>
    /// Validates required shared object-storage settings before an S3-compatible provider is used.
    /// </summary>
    public static class S3CompatibleObjectStorageOptionsValidator
    {
        /// <summary>
        /// Ensures the base S3-compatible object-storage settings are configured.
        /// </summary>
        public static void ValidateBaseConfiguration(ObjectStorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                throw new InvalidOperationException("Object storage service URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.AccessKey))
            {
                throw new InvalidOperationException("Object storage access key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.SecretKey))
            {
                throw new InvalidOperationException("Object storage secret key is not configured.");
            }
        }
    }
}
