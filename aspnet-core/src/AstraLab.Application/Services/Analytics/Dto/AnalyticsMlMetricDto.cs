namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a compact machine-learning metric row for analytics summaries.
    /// </summary>
    public class AnalyticsMlMetricDto
    {
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
