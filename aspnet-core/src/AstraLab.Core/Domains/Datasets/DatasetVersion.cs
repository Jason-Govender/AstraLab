using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents a tenant-scoped version of a dataset for lineage and processing history.
    /// </summary>
    public class DatasetVersion : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// Gets or sets the tenant that owns the dataset version.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset.
        /// </summary>
        public Dataset Dataset { get; set; }

        /// <summary>
        /// Gets or sets the sequential version number within the dataset.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the version type.
        /// </summary>
        public DatasetVersionType VersionType { get; set; }

        /// <summary>
        /// Gets or sets the dataset version status.
        /// </summary>
        public DatasetVersionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the parent version identifier when this version is derived from another version.
        /// </summary>
        public long? ParentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset version.
        /// </summary>
        public DatasetVersion ParentVersion { get; set; }

        /// <summary>
        /// Gets or sets the row count when known.
        /// </summary>
        public int? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the column count when known.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized schema payload when known.
        /// </summary>
        public string SchemaJson { get; set; }

        /// <summary>
        /// Gets or sets the dataset version size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the persisted columns recorded for the dataset version.
        /// </summary>
        public ICollection<DatasetColumn> Columns { get; set; } = new List<DatasetColumn>();

        /// <summary>
        /// Gets or sets the immutable raw file reference for this dataset version.
        /// </summary>
        public DatasetFile RawFile { get; set; }
    }
}
