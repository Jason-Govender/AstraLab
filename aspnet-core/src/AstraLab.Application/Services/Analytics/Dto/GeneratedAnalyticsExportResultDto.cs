namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents the result of generating a persisted analytics export.
    /// </summary>
    public class GeneratedAnalyticsExportResultDto
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the export.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the included machine learning experiment identifier when present.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the persisted report associated with the export when one was used or created.
        /// </summary>
        public ReportRecordDto Report { get; set; }

        /// <summary>
        /// Gets or sets the persisted export reference.
        /// </summary>
        public AnalyticsExportDto Export { get; set; }
    }
}
