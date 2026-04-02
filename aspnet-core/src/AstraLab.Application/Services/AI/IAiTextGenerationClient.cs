using System.Threading.Tasks;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Sends prompt payloads to a text-generation provider.
    /// </summary>
    public interface IAiTextGenerationClient
    {
        /// <summary>
        /// Generates text for the supplied request.
        /// </summary>
        Task<AiTextGenerationResult> GenerateTextAsync(AiTextGenerationRequest request);
    }
}
