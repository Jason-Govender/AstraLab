using System;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a compact persisted AI finding preview used in analytics summaries.
    /// </summary>
    public class AiFindingPreviewDto
    {
        /// <summary>
        /// Gets or sets the source label for the finding.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the finding title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the compact content preview.
        /// </summary>
        public string ContentPreview { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
