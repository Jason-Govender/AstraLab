using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the structured inputs required to build a dataset AI prompt.
    /// </summary>
    public class AiPromptBuildRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether this prompt is for an automatic profiling-triggered insight.
        /// </summary>
        public bool IsAutomaticProfilingInsight { get; set; }

        /// <summary>
        /// Gets or sets the requested AI response type.
        /// </summary>
        public AIResponseType ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the base dataset context.
        /// </summary>
        public AiDatasetContext DatasetContext { get; set; }

        /// <summary>
        /// Gets or sets the optional enrichment context.
        /// </summary>
        public AiDatasetInsightContext EnrichmentContext { get; set; }

        /// <summary>
        /// Gets or sets the optional natural-language question.
        /// </summary>
        public string UserQuestion { get; set; }
    }
}
