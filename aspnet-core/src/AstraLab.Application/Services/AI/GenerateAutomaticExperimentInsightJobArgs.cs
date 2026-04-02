namespace AstraLab.Services.AI
{
    /// <summary>
    /// Carries the tenant-owned machine learning experiment context required to generate an automatic AI insight.
    /// </summary>
    public class GenerateAutomaticExperimentInsightJobArgs
    {
        /// <summary>
        /// Gets or sets the machine learning experiment identifier.
        /// </summary>
        public long MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that the experiment ran against.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier that owns the experiment.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the owner user identifier that owns the backing dataset.
        /// </summary>
        public long OwnerUserId { get; set; }
    }
}
