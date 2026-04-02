namespace AstraLab.Services.AI
{
    /// <summary>
    /// Defines reusable defaults for dataset AI prompting and enrichment.
    /// </summary>
    public static class AiDatasetGenerationDefaults
    {
        /// <summary>
        /// The maximum number of high-signal columns included in enrichment payloads.
        /// </summary>
        public const int MaxHighSignalColumns = 5;

        /// <summary>
        /// The maximum number of recent transformations included in enrichment payloads.
        /// </summary>
        public const int MaxRecentTransformations = 5;

        /// <summary>
        /// The maximum number of prior assistant turns replayed into Q&amp;A prompts.
        /// </summary>
        public const int MaxConversationResponses = 12;
    }
}
