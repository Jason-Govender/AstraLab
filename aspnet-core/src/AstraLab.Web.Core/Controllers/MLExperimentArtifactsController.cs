using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Services.ML;
using Microsoft.AspNetCore.Mvc;

namespace AstraLab.Controllers
{
    /// <summary>
    /// Provides authenticated downloads for persisted ML model artifacts.
    /// </summary>
    [ApiController]
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    [Route("api/services/app/ml/experiments")]
    public class MLExperimentArtifactsController : AstraLabControllerBase
    {
        private readonly IMLArtifactAccessService _mlArtifactAccessService;
        private readonly IAbpSession _abpSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLExperimentArtifactsController"/> class.
        /// </summary>
        public MLExperimentArtifactsController(
            IMLArtifactAccessService mlArtifactAccessService,
            IAbpSession abpSession)
        {
            _mlArtifactAccessService = mlArtifactAccessService;
            _abpSession = abpSession;
        }

        /// <summary>
        /// Downloads the stored model artifact for the selected experiment.
        /// </summary>
        [HttpGet("{id:long}/artifact/download")]
        public async Task<IActionResult> DownloadArtifactAsync(long id)
        {
            try
            {
                var download = await _mlArtifactAccessService.OpenDownloadAsync(id, GetRequiredTenantId(), _abpSession.GetUserId());
                return File(download.Content, download.ContentType ?? "application/octet-stream", download.FileName);
            }
            catch (UserFriendlyException exception)
            {
                return NotFound(new { message = exception.Message });
            }
        }

        /// <summary>
        /// Gets the current tenant identifier or throws when the host context is used.
        /// </summary>
        private int GetRequiredTenantId()
        {
            if (!_abpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for ML experiment operations.");
            }

            return _abpSession.TenantId.Value;
        }
    }
}
