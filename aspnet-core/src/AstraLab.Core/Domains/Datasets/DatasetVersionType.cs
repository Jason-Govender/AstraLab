namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the type of dataset version stored for a dataset.
    /// </summary>
    public enum DatasetVersionType
    {
        /// <summary>
        /// The initial raw version created directly from source data.
        /// </summary>
        Raw = 1,

        /// <summary>
        /// A derived version produced after processing the source data.
        /// </summary>
        Processed = 2
    }
}
