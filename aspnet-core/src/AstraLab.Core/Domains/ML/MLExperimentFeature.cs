using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents a selected feature column for a machine learning experiment.
    /// </summary>
    public class MLExperimentFeature : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// Gets or sets the tenant that owns the feature selection row.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the experiment identifier that owns the feature row.
        /// </summary>
        public long MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the experiment that owns the feature row.
        /// </summary>
        public MLExperiment MLExperiment { get; set; }

        /// <summary>
        /// Gets or sets the selected dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the selected dataset column.
        /// </summary>
        public DatasetColumn DatasetColumn { get; set; }

        /// <summary>
        /// Gets or sets the stable ordinal of the feature within the experiment input set.
        /// </summary>
        public int Ordinal { get; set; }
    }
}
