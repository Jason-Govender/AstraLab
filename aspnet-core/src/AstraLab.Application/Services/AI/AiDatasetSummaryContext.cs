using System;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the dataset-level summary included in AI context assembly.
    /// </summary>
    public class AiDatasetSummaryContext
    {
        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the dataset display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the compact dataset description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dataset description was truncated.
        /// </summary>
        public bool DescriptionWasTruncated { get; set; }

        /// <summary>
        /// Gets or sets the source dataset format.
        /// </summary>
        public DatasetFormat SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the dataset lifecycle status.
        /// </summary>
        public DatasetStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the owning user identifier.
        /// </summary>
        public long OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the compact original file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the original file name was truncated.
        /// </summary>
        public bool OriginalFileNameWasTruncated { get; set; }

        /// <summary>
        /// Gets or sets the dataset creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
