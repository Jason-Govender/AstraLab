using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents a named performance metric for a trained machine learning model.
    /// </summary>
    public class MLModelMetric : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the metric name.
        /// </summary>
        public const int MaxMetricNameLength = 100;

        /// <summary>
        /// Gets or sets the tenant that owns the metric row.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the model identifier that owns the metric.
        /// </summary>
        public long MLModelId { get; set; }

        /// <summary>
        /// Gets or sets the model that owns the metric.
        /// </summary>
        public MLModel MLModel { get; set; }

        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; }

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        public decimal MetricValue { get; set; }
    }
}
