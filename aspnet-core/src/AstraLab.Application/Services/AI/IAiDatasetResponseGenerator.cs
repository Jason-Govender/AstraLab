using System.Threading.Tasks;
using AstraLab.Core.Domains.AI;
using AstraLab.Services.AI.Dto;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Orchestrates grounded dataset AI generation and persistence.
    /// </summary>
    public interface IAiDatasetResponseGenerator
    {
        /// <summary>
        /// Generates and persists a dataset-scoped AI response.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateAsync(
            AIResponseType responseType,
            long datasetVersionId,
            int tenantId,
            long ownerUserId,
            string userQuery = null,
            long? conversationId = null);

        /// <summary>
        /// Generates and persists a profiling-triggered automatic insight for the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateAutomaticInsightAsync(
            long datasetVersionId,
            long datasetProfileId,
            int tenantId,
            long ownerUserId);
    }
}
