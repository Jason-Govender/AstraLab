using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents a tenant-owned dataset record that acts as the root for future ingestion metadata.
    /// </summary>
    public class Dataset : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the dataset name.
        /// </summary>
        public const int MaxNameLength = 300;

        /// <summary>
        /// The maximum allowed length for the dataset description.
        /// </summary>
        public const int MaxDescriptionLength = 1000;

        /// <summary>
        /// The maximum allowed length for the original file name.
        /// </summary>
        public const int MaxOriginalFileNameLength = 300;

        /// <summary>
        /// Gets or sets the tenant that owns the dataset.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the dataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional user-provided description for the dataset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the source format used when the dataset was uploaded.
        /// </summary>
        public DatasetFormat SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the current lifecycle status for the dataset.
        /// </summary>
        public DatasetStatus Status { get; set; } = DatasetStatus.Uploaded;

        /// <summary>
        /// Gets or sets the owning user identifier for the dataset.
        /// </summary>
        public long OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the current active dataset version identifier.
        /// </summary>
        public long? CurrentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the current active dataset version.
        /// </summary>
        public DatasetVersion CurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets all versions recorded for the dataset.
        /// </summary>
        public ICollection<DatasetVersion> Versions { get; set; } = new List<DatasetVersion>();
    }
}
