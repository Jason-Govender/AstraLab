using System.IO;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Represents a validated ML artifact download payload.
    /// </summary>
    public class MLArtifactDownloadResult
    {
        /// <summary>
        /// Gets or sets the artifact content stream.
        /// </summary>
        public Stream Content { get; set; }

        /// <summary>
        /// Gets or sets the file name that should be used for downloads.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional content type for the artifact.
        /// </summary>
        public string ContentType { get; set; }
    }
}
