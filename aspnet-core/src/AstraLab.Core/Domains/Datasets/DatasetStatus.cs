namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the lifecycle status of a dataset during ingestion and later processing.
    /// </summary>
    public enum DatasetStatus
    {
        /// <summary>
        /// The dataset has been registered after upload and awaits further processing.
        /// </summary>
        Uploaded = 1,

        /// <summary>
        /// The dataset is currently being profiled.
        /// </summary>
        Profiling = 2,

        /// <summary>
        /// The dataset is ready for downstream use.
        /// </summary>
        Ready = 3,

        /// <summary>
        /// Dataset processing failed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The dataset has been archived.
        /// </summary>
        Archived = 5
    }
}
