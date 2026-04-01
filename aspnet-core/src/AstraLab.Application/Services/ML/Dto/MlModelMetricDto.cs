namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a model metric returned to the frontend.
    /// </summary>
    public class MlModelMetricDto
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
