namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the configured Groq text-generation settings.
    /// </summary>
    public class GroqAiOptions
    {
        /// <summary>
        /// Gets or sets the Groq base URL.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Groq API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the default model name.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum output tokens.
        /// </summary>
        public int MaxOutputTokens { get; set; }

        /// <summary>
        /// Gets or sets the optional reasoning effort.
        /// </summary>
        public string ReasoningEffort { get; set; }
    }
}
