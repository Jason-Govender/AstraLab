namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Renders canonical analytics report content to PDF bytes.
    /// </summary>
    public interface IAnalyticsReportPdfRenderer
    {
        /// <summary>
        /// Renders the supplied report title and canonical HTML content to a PDF payload.
        /// </summary>
        byte[] Render(string title, string htmlContent);
    }
}
