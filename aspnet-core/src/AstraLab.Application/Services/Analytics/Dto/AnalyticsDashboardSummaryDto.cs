namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the compact dashboard-ready analytics overview for a dataset version.
    /// </summary>
    public class AnalyticsDashboardSummaryDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the data-health score when available.
        /// </summary>
        public decimal? DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the profiled row count when available.
        /// </summary>
        public long? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the dataset-version column count when available.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of high-risk columns surfaced in the quality section.
        /// </summary>
        public int HighRiskColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of recent transformations included in the summary.
        /// </summary>
        public int RecentTransformationCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persisted analytics insight records.
        /// </summary>
        public int StoredInsightCount { get; set; }

        /// <summary>
        /// Gets or sets the number of persisted AI responses.
        /// </summary>
        public int StoredAiResponseCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic AI insight data is available.
        /// </summary>
        public bool HasAutomaticAiInsight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a completed machine-learning experiment is available.
        /// </summary>
        public bool HasCompletedMlExperiment { get; set; }

        /// <summary>
        /// Gets or sets the primary machine-learning metric name when available.
        /// </summary>
        public string PrimaryMetricName { get; set; }

        /// <summary>
        /// Gets or sets the primary machine-learning metric value when available.
        /// </summary>
        public decimal? PrimaryMetricValue { get; set; }

        /// <summary>
        /// Gets or sets the number of surfaced machine-learning warnings.
        /// </summary>
        public int MlWarningCount { get; set; }
    }
}
