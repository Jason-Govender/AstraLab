using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the unified dataset-version-scoped analytics summary returned to dashboards and reports.
    /// </summary>
    public class DatasetAnalyticsSummaryDto
    {
        /// <summary>
        /// Gets or sets the parent dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset name.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// Gets or sets the dataset source format.
        /// </summary>
        public DatasetFormat SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the dataset status.
        /// </summary>
        public DatasetStatus DatasetStatus { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version number.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the dataset version type.
        /// </summary>
        public DatasetVersionType VersionType { get; set; }

        /// <summary>
        /// Gets or sets the dataset version status.
        /// </summary>
        public DatasetVersionStatus VersionStatus { get; set; }

        /// <summary>
        /// Gets or sets the dataset-version column count when available.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the dataset-version size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the dataset-quality highlights.
        /// </summary>
        public DatasetQualityHighlightsDto QualityHighlights { get; set; }

        /// <summary>
        /// Gets or sets the recent transformation outcomes.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<TransformationOutcomeSummaryDto> TransformationOutcomes { get; set; } = new System.Collections.Generic.List<TransformationOutcomeSummaryDto>();

        /// <summary>
        /// Gets or sets the persisted AI findings summary.
        /// </summary>
        public AiFindingsSummaryDto AiFindings { get; set; }

        /// <summary>
        /// Gets or sets the latest completed machine-learning experiment highlights.
        /// </summary>
        public MlExperimentHighlightsDto MlExperimentHighlights { get; set; }

        /// <summary>
        /// Gets or sets the compact dashboard summary.
        /// </summary>
        public AnalyticsDashboardSummaryDto DashboardSummary { get; set; }

        /// <summary>
        /// Gets or sets the optional AI-generated stakeholder narrative.
        /// </summary>
        public AnalyticsNarrativeDto Narrative { get; set; }
    }
}
