using System.Threading.Tasks;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraLab.Controllers
{
    /// <summary>
    /// Receives internal callback notifications from the Python ML executor service.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("api/services/app/ml-experiments/callbacks")]
    public class MLExperimentCallbacksController : AstraLabControllerBase
    {
        private readonly IMLExperimentExecutionManager _mlExperimentExecutionManager;
        private readonly MLExecutionOptions _mlExecutionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExperimentCallbacksController"/> class.
        /// </summary>
        public MLExperimentCallbacksController(
            IMLExperimentExecutionManager mlExperimentExecutionManager,
            MLExecutionOptions mlExecutionOptions)
        {
            _mlExperimentExecutionManager = mlExperimentExecutionManager;
            _mlExecutionOptions = mlExecutionOptions;
        }

        /// <summary>
        /// Applies a completion callback from the ML executor.
        /// </summary>
        [HttpPost("experiment-completed")]
        public async Task<IActionResult> CompleteAsync([FromBody] CompleteMlExperimentCallbackRequest input)
        {
            if (!IsAuthorizedRequest())
            {
                return Unauthorized();
            }

            await _mlExperimentExecutionManager.CompleteAsync(input);
            return Ok();
        }

        /// <summary>
        /// Applies a failure callback from the ML executor.
        /// </summary>
        [HttpPost("experiment-failed")]
        public async Task<IActionResult> FailAsync([FromBody] FailMlExperimentCallbackRequest input)
        {
            if (!IsAuthorizedRequest())
            {
                return Unauthorized();
            }

            await _mlExperimentExecutionManager.FailAsync(input);
            return Ok();
        }

        private bool IsAuthorizedRequest()
        {
            return Request.Headers.TryGetValue(MLHttpJobDispatcher.SharedSecretHeaderName, out var secretHeader) &&
                   string.Equals(secretHeader.ToString(), _mlExecutionOptions.SharedSecret, System.StringComparison.Ordinal);
        }
    }
}
