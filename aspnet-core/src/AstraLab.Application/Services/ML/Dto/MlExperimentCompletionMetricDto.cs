namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a model metric sent by the ML executor callback.
    /// </summary>
    public class MlExperimentCompletionMetricDto
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
