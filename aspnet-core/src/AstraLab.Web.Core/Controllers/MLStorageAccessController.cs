using System.Threading.Tasks;
using Abp.UI;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.ML.Storage;
using AstraLab.Web.Core.ML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraLab.Controllers
{
    /// <summary>
    /// Exposes short-lived internal endpoints that the ML executor uses to transfer datasets and artifacts.
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("api/internal/ml-storage")]
    public class MLStorageAccessController : AstraLabControllerBase
    {
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IMLArtifactStorage _mlArtifactStorage;
        private readonly MLExecutorFileAccessTokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLStorageAccessController"/> class.
        /// </summary>
        public MLStorageAccessController(
            IRawDatasetStorage rawDatasetStorage,
            IMLArtifactStorage mlArtifactStorage,
            MLExecutorFileAccessTokenService tokenService)
        {
            _rawDatasetStorage = rawDatasetStorage;
            _mlArtifactStorage = mlArtifactStorage;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Downloads a stored dataset file for the ML executor.
        /// </summary>
        [HttpGet("datasets")]
        public async Task<IActionResult> DownloadDatasetAsync([FromQuery] string token)
        {
            MLExecutorFileAccessTokenService.FileAccessTokenPayload payload;
            try
            {
                payload = _tokenService.ValidateDatasetDownloadToken(token);
            }
            catch (UserFriendlyException)
            {
                return Unauthorized();
            }

            var stream = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
            {
                StorageProvider = payload.StorageProvider,
                StorageKey = payload.StorageKey
            });

            return File(stream, "application/octet-stream");
        }

        /// <summary>
        /// Uploads a generated ML artifact file from the executor.
        /// </summary>
        [HttpPut("artifacts")]
        public async Task<IActionResult> UploadArtifactAsync([FromQuery] string token)
        {
            MLExecutorFileAccessTokenService.FileAccessTokenPayload payload;
            try
            {
                payload = _tokenService.ValidateArtifactUploadToken(token);
            }
            catch (UserFriendlyException)
            {
                return Unauthorized();
            }

            await _mlArtifactStorage.StoreAsync(new StoreMlArtifactRequest
            {
                StorageProvider = payload.StorageProvider,
                StorageKey = payload.StorageKey,
                Content = Request.Body
            });

            return NoContent();
        }
    }
}
