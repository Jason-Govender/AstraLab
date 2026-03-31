namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Describes the logical storage category for a persisted dataset version file.
    /// </summary>
    public enum DatasetVersionFileKind
    {
        /// <summary>
        /// The stored file is the original raw dataset content for a raw version.
        /// </summary>
        Raw = 1,

        /// <summary>
        /// The stored file is a processed dataset artifact for a derived version.
        /// </summary>
        Processed = 2
    }
}
