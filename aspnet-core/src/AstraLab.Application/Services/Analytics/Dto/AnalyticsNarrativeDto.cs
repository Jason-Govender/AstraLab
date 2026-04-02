namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the optional stakeholder-facing narrative generated for an analytics summary.
    /// </summary>
    public class AnalyticsNarrativeDto
    {
        /// <summary>
        /// Gets or sets the narrative generation status.
        /// </summary>
        public AnalyticsNarrativeStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the generated narrative content when available.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the non-fatal failure message when narrative generation fails.
        /// </summary>
        public string FailureMessage { get; set; }
    }
}
