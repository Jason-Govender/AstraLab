namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the background-job payload used to generate automatic insights after profiling.
    /// </summary>
    public class GenerateAutomaticDatasetInsightJobArgs
    {
        /// <summary>
        /// Gets or sets the profiled dataset version identifier.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the current dataset profile identifier that triggered the job.
        /// </summary>
        public long DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the tenant that owns the dataset version.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset owner that should be used for access validation.
        /// </summary>
        public long OwnerUserId { get; set; }
    }
}
