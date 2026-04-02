using System.Threading.Tasks;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds structured, size-controlled dataset context for future AI workflows.
    /// </summary>
    public interface IAiDatasetContextBuilder
    {
        /// <summary>
        /// Builds the AI dataset context for the specified dataset version and owner scope.
        /// </summary>
        Task<AiDatasetContext> BuildAsync(long datasetVersionId, int tenantId, long ownerUserId);
    }
}
