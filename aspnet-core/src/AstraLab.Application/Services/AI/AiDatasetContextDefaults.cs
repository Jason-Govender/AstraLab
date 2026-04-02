namespace AstraLab.Services.AI
{
    /// <summary>
    /// Defines the default limits used when shaping dataset context for future AI workflows.
    /// </summary>
    public static class AiDatasetContextDefaults
    {
        /// <summary>
        /// The maximum number of columns that can receive full profiling detail before pruning is applied.
        /// </summary>
        public const int MaxColumnsWithFullDetail = 20;

        /// <summary>
        /// The maximum number of columns that can retain detailed profiling insights in compact mode.
        /// </summary>
        public const int MaxProfiledColumnsInCompactSummary = 8;

        /// <summary>
        /// The maximum number of characters kept from dataset descriptions.
        /// </summary>
        public const int MaxDatasetDescriptionLength = 400;

        /// <summary>
        /// The maximum number of characters kept from serialized schema payloads.
        /// </summary>
        public const int MaxSchemaPreviewLength = 1200;

        /// <summary>
        /// The maximum number of characters kept from original file names in compact context summaries.
        /// </summary>
        public const int MaxOriginalFileNameLength = 200;
    }
}
