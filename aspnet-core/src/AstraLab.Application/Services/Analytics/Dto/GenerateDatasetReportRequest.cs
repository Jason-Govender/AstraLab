namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Requests generation of a stakeholder-facing dataset analytics report.
    /// </summary>
    public class GenerateDatasetReportRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier to report on.
        /// </summary>
        public long DatasetVersionId { get; set; }
    }
}
