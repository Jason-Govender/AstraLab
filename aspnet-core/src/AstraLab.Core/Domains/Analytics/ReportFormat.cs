namespace AstraLab.Core.Domains.Analytics
{
    /// <summary>
    /// Classifies the persisted report content format.
    /// </summary>
    public enum ReportFormat
    {
        /// <summary>
        /// Represents plain text content.
        /// </summary>
        PlainText = 1,

        /// <summary>
        /// Represents markdown content.
        /// </summary>
        Markdown = 2,

        /// <summary>
        /// Represents HTML content.
        /// </summary>
        Html = 3,

        /// <summary>
        /// Represents JSON content.
        /// </summary>
        Json = 4
    }
}
