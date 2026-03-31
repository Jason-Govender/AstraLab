namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Describes the supported transformation actions for processed dataset versions.
    /// </summary>
    public enum DatasetTransformationType
    {
        /// <summary>
        /// Removes duplicate rows from the source dataset version.
        /// </summary>
        RemoveDuplicates = 1,

        /// <summary>
        /// Applies a missing-value handling strategy to the source dataset version.
        /// </summary>
        HandleMissingValues = 2,

        /// <summary>
        /// Converts one or more source columns to a new data type.
        /// </summary>
        ConvertDataType = 3,

        /// <summary>
        /// Filters rows from the source dataset version by a configured predicate.
        /// </summary>
        FilterRows = 4,

        /// <summary>
        /// Aggregates rows from the source dataset version into summary results.
        /// </summary>
        Aggregate = 5
    }
}
