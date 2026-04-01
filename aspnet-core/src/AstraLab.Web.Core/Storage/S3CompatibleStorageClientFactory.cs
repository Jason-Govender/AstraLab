using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using AstraLab.Services.Storage;

namespace AstraLab.Web.Core.Storage
{
    /// <summary>
    /// Creates configured S3-compatible clients for object-storage operations.
    /// </summary>
    public static class S3CompatibleStorageClientFactory
    {
        /// <summary>
        /// Creates an <see cref="IAmazonS3"/> instance from the configured shared object-storage settings.
        /// </summary>
        public static IAmazonS3 Create(ObjectStorageOptions options)
        {
            S3CompatibleObjectStorageOptionsValidator.ValidateBaseConfiguration(options);

            var configuration = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = options.ForcePathStyle
            };

            // When a custom service URL is supplied, prefer it over AWS region-derived
            // endpoints so S3-compatible providers like Cloudflare R2 are addressed correctly.
            if (string.IsNullOrWhiteSpace(options.ServiceUrl) && !string.IsNullOrWhiteSpace(options.Region))
            {
                configuration.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                configuration);
        }
    }
}
