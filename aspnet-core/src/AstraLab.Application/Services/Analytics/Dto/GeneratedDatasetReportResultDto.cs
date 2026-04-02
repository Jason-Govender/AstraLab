namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the result of generating a persisted dataset analytics report.
    /// </summary>
    public class GeneratedDatasetReportResultDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the report.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the included machine learning experiment identifier when present.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the persisted report.
        /// </summary>
        public ReportRecordDto Report { get; set; }
    }
}
