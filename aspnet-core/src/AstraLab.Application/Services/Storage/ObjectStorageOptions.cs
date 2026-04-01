namespace AstraLab.Services.Storage
{
    /// <summary>
    /// Defines the shared object-storage settings used by dataset and ML artifact storage.
    /// </summary>
    public class ObjectStorageOptions
    {
        /// <summary>
        /// Gets or sets the object-storage service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the configured object-storage region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the object-storage access key.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the object-storage secret key.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether path-style addressing should be used.
        /// </summary>
        public bool ForcePathStyle { get; set; } = true;

        /// <summary>
        /// Gets or sets the bucket used for dataset files.
        /// </summary>
        public string DatasetBucketName { get; set; }

        /// <summary>
        /// Gets or sets the object prefix used for dataset files.
        /// </summary>
        public string DatasetKeyPrefix { get; set; } = "datasets";

        /// <summary>
        /// Gets or sets the bucket used for ML artifact files.
        /// </summary>
        public string MlArtifactBucketName { get; set; }

        /// <summary>
        /// Gets or sets the object prefix used for ML artifact files.
        /// </summary>
        public string MlArtifactKeyPrefix { get; set; } = "ml-artifacts";

        /// <summary>
        /// Gets or sets the temporary URL lifetime in seconds.
        /// </summary>
        public int PresignedUrlTtlSeconds { get; set; } = 900;
    }
}
