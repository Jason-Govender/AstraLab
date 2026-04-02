namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents a provider-neutral text-generation result.
    /// </summary>
    public class AiTextGenerationResult
    {
        /// <summary>
        /// Gets or sets the final generated text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the provider name that generated the text.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the provider model name when available.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the provider response identifier when available.
        /// </summary>
        public string ProviderResponseId { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized usage metadata.
        /// </summary>
        public string UsageJson { get; set; }
    }
}
