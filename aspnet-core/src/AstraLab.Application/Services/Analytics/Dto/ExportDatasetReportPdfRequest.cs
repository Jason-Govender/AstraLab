namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Requests generation of a PDF export for a dataset analytics report.
    /// </summary>
    public class ExportDatasetReportPdfRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the export.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional existing report identifier to export.
        /// </summary>
        public long? ReportRecordId { get; set; }
    }
}
