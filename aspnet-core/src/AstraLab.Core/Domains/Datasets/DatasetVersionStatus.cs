namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the lifecycle status of a dataset version.
    /// </summary>
    public enum DatasetVersionStatus
    {
        /// <summary>
        /// The dataset version has been created but is not yet active.
        /// </summary>
        Draft = 1,

        /// <summary>
        /// The dataset version is the active usable version.
        /// </summary>
        Active = 2,

        /// <summary>
        /// The dataset version has been replaced by a newer active version.
        /// </summary>
        Superseded = 3,

        /// <summary>
        /// Processing for the dataset version failed.
        /// </summary>
        Failed = 4
    }
}
