using System.Threading.Tasks;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Dispatches accepted ML experiments to the external executor service.
    /// </summary>
    public interface IMLJobDispatcher
    {
        /// <summary>
        /// Dispatches the specified experiment payload to the external executor.
        /// </summary>
        Task DispatchAsync(DispatchMlExperimentRequest request);
    }
}
