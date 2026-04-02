namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the generated provider-neutral prompt payload.
    /// </summary>
    public class AiPromptBuildResult
    {
        /// <summary>
        /// Gets or sets the system-level instructions.
        /// </summary>
        public string SystemInstructions { get; set; }

        /// <summary>
        /// Gets or sets the final user prompt.
        /// </summary>
        public string UserMessage { get; set; }
    }
}
