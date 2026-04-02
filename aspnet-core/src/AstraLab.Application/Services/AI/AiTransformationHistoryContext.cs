using System;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents a compact transformation-history row used in AI enrichment.
    /// </summary>
    public class AiTransformationHistoryContext
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
        /// Gets or sets the result dataset version identifier.
        /// </summary>
        public long? ResultDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the execution order.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the execution timestamp.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the compact execution summary payload when available.
        /// </summary>
        public string SummaryJson { get; set; }
    }
}
