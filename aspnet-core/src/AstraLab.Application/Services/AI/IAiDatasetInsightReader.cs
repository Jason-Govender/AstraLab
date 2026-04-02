using System.Threading.Tasks;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Loads compact read-only enrichment data for dataset AI prompts.
    /// </summary>
    public interface IAiDatasetInsightReader
    {
        /// <summary>
        /// Reads compact enrichment context for the specified dataset version.
        /// </summary>
        Task<AiDatasetInsightContext> ReadAsync(long datasetVersionId, int tenantId, long ownerUserId);
    }
}
