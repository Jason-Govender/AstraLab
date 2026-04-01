using System.Collections.Generic;

namespace AstraLab.Services.Storage
{
    /// <summary>
    /// Represents the outcome of a legacy local-storage backfill run.
    /// </summary>
    public class ObjectStorageBackfillResult
    {
        /// <summary>
        /// Gets or sets the number of dataset files migrated successfully.
        /// </summary>
        public int MigratedDatasetFileCount { get; set; }

        /// <summary>
        /// Gets or sets the number of ML artifacts migrated successfully.
        /// </summary>
        public int MigratedMlArtifactCount { get; set; }

        /// <summary>
        /// Gets or sets the errors captured while processing the backfill batch.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
