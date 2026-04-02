using System.Threading.Tasks;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds structured machine learning experiment context for grounded AI workflows.
    /// </summary>
    public interface IAiMlExperimentContextBuilder
    {
        /// <summary>
        /// Builds the machine learning experiment context for the selected owner-scoped experiment.
        /// </summary>
        Task<AiMlExperimentContext> BuildAsync(long mlExperimentId, int tenantId, long ownerUserId);
    }
}
