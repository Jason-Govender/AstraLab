using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents a reproducible transformation step executed from one dataset version to another.
    /// </summary>
    public class DatasetTransformation : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The database column type used for serialized transformation configuration payloads.
        /// </summary>
        public const string ConfigurationJsonColumnType = "text";

        /// <summary>
        /// The database column type used for serialized transformation summary payloads.
        /// </summary>
        public const string SummaryJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the transformation record.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the source dataset version identifier the transformation ran against.
        /// </summary>
        public long SourceDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the source dataset version the transformation ran against.
        /// </summary>
        public DatasetVersion SourceDatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the resulting processed dataset version identifier when one has been created.
        /// </summary>
        public long? ResultDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the resulting processed dataset version when one has been created.
        /// </summary>
        public DatasetVersion ResultDatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType { get; set; }

        /// <summary>
        /// Gets or sets the serialized configuration payload that makes the transformation reproducible.
        /// </summary>
        public string ConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the ordered position of this transformation for a given source dataset version.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the transformation executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the serialized summary payload when extra execution notes are available.
        /// </summary>
        public string SummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the persisted AI responses optionally linked to this transformation.
        /// </summary>
        public ICollection<AIResponse> AIResponses { get; set; } = new List<AIResponse>();
    }
}
