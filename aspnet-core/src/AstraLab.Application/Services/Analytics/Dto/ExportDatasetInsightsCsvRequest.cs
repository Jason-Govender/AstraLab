namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Requests generation of a CSV export containing structured dataset analytics highlights.
    /// </summary>
    public class ExportDatasetInsightsCsvRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the export.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional existing report identifier to associate with the export.
        /// </summary>
        public long? ReportRecordId { get; set; }
    }
}
