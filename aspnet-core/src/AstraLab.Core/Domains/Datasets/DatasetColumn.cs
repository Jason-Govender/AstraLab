using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents persisted structural metadata for a dataset version column.
    /// </summary>
    public class DatasetColumn : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the column name.
        /// </summary>
        public const int MaxNameLength = 300;

        /// <summary>
        /// The maximum allowed length for the declared or inferred data type.
        /// </summary>
        public const int MaxDataTypeLength = 100;

        /// <summary>
        /// Gets or sets the tenant that owns the dataset column.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that owns the column.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that owns the column.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the declared or inferred data type name.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data type was inferred rather than explicitly declared.
        /// </summary>
        public bool IsDataTypeInferred { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column within the dataset version.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the null value count when known.
        /// </summary>
        public long? NullCount { get; set; }

        /// <summary>
        /// Gets or sets the distinct value count when known.
        /// </summary>
        public long? DistinctCount { get; set; }
    }
}
