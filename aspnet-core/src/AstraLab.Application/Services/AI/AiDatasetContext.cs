using System.Collections.Generic;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the structured dataset-version context assembled for future AI workflows.
    /// </summary>
    public class AiDatasetContext
    {
        /// <summary>
        /// Gets or sets the dataset-level summary.
        /// </summary>
        public AiDatasetSummaryContext Dataset { get; set; }

        /// <summary>
        /// Gets or sets the selected dataset version summary.
        /// </summary>
        public AiDatasetVersionContext Version { get; set; }

        /// <summary>
        /// Gets or sets the schema-level summary.
        /// </summary>
        public AiSchemaContext Schema { get; set; }

        /// <summary>
        /// Gets or sets the dataset-level profiling summary when available.
        /// </summary>
        public AiProfilingContext Profiling { get; set; }

        /// <summary>
        /// Gets or sets the ordered column context collection.
        /// </summary>
        public IReadOnlyList<AiColumnContext> Columns { get; set; }

        /// <summary>
        /// Gets or sets the number of columns that retained detailed profiling context.
        /// </summary>
        public int DetailedColumnCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed column context was pruned for token control.
        /// </summary>
        public bool IsColumnContextPruned { get; set; }
    }
}
