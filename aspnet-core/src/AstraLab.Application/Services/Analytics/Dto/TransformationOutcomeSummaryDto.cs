using System;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a concise transformation outcome row for analytics summaries.
    /// </summary>
    public class TransformationOutcomeSummaryDto
    {
        /// <summary>
        /// Gets or sets the dataset transformation identifier.
        /// </summary>
        public long DatasetTransformationId { get; set; }

        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType { get; set; }

        /// <summary>
        /// Gets or sets the source dataset version identifier.
        /// </summary>
        public long SourceDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the result dataset version identifier when available.
        /// </summary>
        public long? ResultDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the execution order within the transformation flow.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the execution time.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the compact stakeholder-facing transformation summary preview.
        /// </summary>
        public string SummaryPreview { get; set; }
    }
}
