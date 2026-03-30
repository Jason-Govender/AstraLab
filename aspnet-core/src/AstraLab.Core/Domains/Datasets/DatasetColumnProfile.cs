using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the current persisted profiling snapshot for a dataset version column.
    /// </summary>
    public class DatasetColumnProfile : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the profiled data type.
        /// </summary>
        public const int MaxInferredDataTypeLength = 100;

        /// <summary>
        /// The column type used for persisted statistics payloads.
        /// </summary>
        public const string StatisticsJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the dataset column profile.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset profile identifier.
        /// </summary>
        public long DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset profile.
        /// </summary>
        public DatasetProfile DatasetProfile { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier that the profile row describes.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column that the profile row describes.
        /// </summary>
        public DatasetColumn DatasetColumn { get; set; }

        /// <summary>
        /// Gets or sets the profiled inferred data type.
        /// </summary>
        public string InferredDataType { get; set; }

        /// <summary>
        /// Gets or sets the profiled null value count.
        /// </summary>
        public long NullCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled distinct value count when it was computed.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized profile statistics payload for future numeric and categorical insights.
        /// </summary>
        public string StatisticsJson { get; set; }
    }
}
