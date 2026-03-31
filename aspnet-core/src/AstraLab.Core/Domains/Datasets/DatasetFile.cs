using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the immutable stored file reference for a dataset version.
    /// </summary>
    public class DatasetFile : FullAuditedEntity<long>, IMustHaveTenant
    {
        public const int MaxStorageProviderLength = 50;
        public const int MaxStorageKeyLength = 500;
        public const int MaxOriginalFileNameLength = 300;
        public const int MaxContentTypeLength = 255;
        public const int ChecksumSha256Length = 64;

        /// <summary>
        /// Gets or sets the tenant that owns the dataset file reference.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that owns the stored file.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that owns the stored file.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the storage provider name used for the stored file.
        /// </summary>
        public string StorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the logical storage key for the stored file.
        /// </summary>
        public string StorageKey { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the optional content type associated with the raw file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the stored file size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the SHA-256 checksum for the stored raw file.
        /// </summary>
        public string ChecksumSha256 { get; set; }
    }
}
