namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents the lifecycle state of a machine learning experiment run.
    /// </summary>
    public enum MLExperimentStatus
    {
        /// <summary>
        /// The experiment has been created but has not started yet.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// The experiment is currently running.
        /// </summary>
        Running = 2,

        /// <summary>
        /// The experiment completed successfully.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// The experiment failed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The experiment was cancelled before execution completed.
        /// </summary>
        Cancelled = 5
    }
}
