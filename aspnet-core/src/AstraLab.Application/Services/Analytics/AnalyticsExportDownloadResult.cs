using System.IO;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Represents a validated analytics export download payload.
    /// </summary>
    public class AnalyticsExportDownloadResult
    {
        /// <summary>
        /// Gets or sets the export content stream.
        /// </summary>
        public Stream Content { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing download file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional persisted content type.
        /// </summary>
        public string ContentType { get; set; }
    }
}
