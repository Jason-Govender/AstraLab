namespace AstraLab.Core.Domains.Analytics
{
    /// <summary>
    /// Classifies the type of exported analytics payload.
    /// </summary>
    public enum AnalyticsExportType
    {
        /// <summary>
        /// Represents a document-style export.
        /// </summary>
        Document = 1,

        /// <summary>
        /// Represents a tabular export.
        /// </summary>
        Spreadsheet = 2,

        /// <summary>
        /// Represents a presentation-style export.
        /// </summary>
        Presentation = 3,

        /// <summary>
        /// Represents a serialized machine-readable export.
        /// </summary>
        DataPackage = 4
    }
}
