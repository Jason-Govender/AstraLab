using System.Threading.Tasks;
using AstraLab.Services.ML.Dto;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Applies executor callback updates to persisted ML experiments.
    /// </summary>
    public interface IMLExperimentExecutionManager
    {
        /// <summary>
        /// Persists a successful completion callback.
        /// </summary>
        Task CompleteAsync(CompleteMlExperimentCallbackRequest input);

        /// <summary>
        /// Persists a failed completion callback.
        /// </summary>
        Task FailAsync(FailMlExperimentCallbackRequest input);
    }
}
