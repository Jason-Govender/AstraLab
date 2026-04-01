using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents the trained model metadata produced by a completed experiment.
    /// </summary>
    public class MLModel : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the model type name.
        /// </summary>
        public const int MaxModelTypeLength = 100;

        /// <summary>
        /// The database column type used for serialized performance summary payloads.
        /// </summary>
        public const string PerformanceSummaryJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the model metadata.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the experiment identifier that produced the model.
        /// </summary>
        public long MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the experiment that produced the model.
        /// </summary>
        public MLExperiment MLExperiment { get; set; }

        /// <summary>
        /// Gets or sets the model type identifier.
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Gets or sets the optional storage provider for the persisted trained-model artifact.
        /// </summary>
        public string ArtifactStorageProvider { get; set; }

        /// <summary>
        /// Gets or sets the optional logical storage key for the persisted trained-model artifact.
        /// </summary>
        public string ArtifactStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized performance summary payload.
        /// </summary>
        public string PerformanceSummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the persisted model metrics.
        /// </summary>
        public ICollection<MLModelMetric> Metrics { get; set; } = new List<MLModelMetric>();

        /// <summary>
        /// Gets or sets the persisted feature importance rows.
        /// </summary>
        public ICollection<MLModelFeatureImportance> FeatureImportances { get; set; } = new List<MLModelFeatureImportance>();
    }
}
