using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents the importance score of a feature column for a trained machine learning model.
    /// </summary>
    public class MLModelFeatureImportance : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// Gets or sets the tenant that owns the feature importance row.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the model identifier that owns the feature importance row.
        /// </summary>
        public long MLModelId { get; set; }

        /// <summary>
        /// Gets or sets the model that owns the feature importance row.
        /// </summary>
        public MLModel MLModel { get; set; }

        /// <summary>
        /// Gets or sets the dataset column identifier the importance score applies to.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column the importance score applies to.
        /// </summary>
        public DatasetColumn DatasetColumn { get; set; }

        /// <summary>
        /// Gets or sets the feature importance score.
        /// </summary>
        public decimal ImportanceScore { get; set; }

        /// <summary>
        /// Gets or sets the ordinal rank of the feature importance result.
        /// </summary>
        public int Rank { get; set; }
    }
}
