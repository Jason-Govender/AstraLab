namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents one persisted machine learning metric included in AI experiment context.
    /// </summary>
    public class AiMlMetricContext
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
