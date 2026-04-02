using System;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the selected dataset version summary included in AI context assembly.
    /// </summary>
    public class AiDatasetVersionContext
    {
        /// <summary>
        /// Gets or sets the dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the parent dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the sequential version number.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the dataset version type.
        /// </summary>
        public DatasetVersionType VersionType { get; set; }

        /// <summary>
        /// Gets or sets the dataset version status.
        /// </summary>
        public DatasetVersionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the parent version identifier when present.
        /// </summary>
        public long? ParentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the persisted row count when known.
        /// </summary>
        public int? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the persisted column count when known.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the stored dataset size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the dataset version creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
