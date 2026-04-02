using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds deterministic task-specific prompts for dataset AI workflows.
    /// </summary>
    public interface IAiPromptBuilder
    {
        /// <summary>
        /// Builds a prompt payload for the requested dataset AI task.
        /// </summary>
        AiPromptBuildResult Build(AiPromptBuildRequest request);
    }
}
