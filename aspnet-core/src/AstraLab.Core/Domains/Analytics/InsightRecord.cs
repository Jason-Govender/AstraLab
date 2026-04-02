using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;

namespace AstraLab.Core.Domains.Analytics
{
    /// <summary>
    /// Represents a persisted analytics insight linked to a dataset version and optional supporting context.
    /// </summary>
    public class InsightRecord : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed title length.
        /// </summary>
        public const int MaxTitleLength = 256;

        /// <summary>
        /// The database column type used for persisted insight content.
        /// </summary>
        public const string ContentColumnType = "text";

        /// <summary>
        /// The database column type used for serialized metadata payloads.
        /// </summary>
        public const string MetadataJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the insight.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the insight.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that anchors the insight.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset profile identifier that informed the insight.
        /// </summary>
        public long? DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset profile that informed the insight.
        /// </summary>
        public DatasetProfile DatasetProfile { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier that informed the insight.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment that informed the insight.
        /// </summary>
        public MLExperiment MLExperiment { get; set; }

        /// <summary>
        /// Gets or sets the optional AI response identifier that originated the insight.
        /// </summary>
        public long? AIResponseId { get; set; }

        /// <summary>
        /// Gets or sets the optional AI response that originated the insight.
        /// </summary>
        public AIResponse AIResponse { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing insight title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the persisted insight content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the insight classification.
        /// </summary>
        public InsightType InsightType { get; set; }

        /// <summary>
        /// Gets or sets the source classification for the insight.
        /// </summary>
        public InsightSourceType InsightSourceType { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Gets or sets the persisted exports derived from this insight.
        /// </summary>
        public ICollection<AnalyticsExport> Exports { get; set; } = new List<AnalyticsExport>();
    }
}
