using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Services.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace AstraLab.Controllers
{
    /// <summary>
    /// Provides authenticated downloads for persisted analytics export files.
    /// </summary>
    [ApiController]
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    [Route("api/services/app/analytics/exports")]
    public class AnalyticsExportsController : AstraLabControllerBase
    {
        private readonly IAnalyticsExportAccessService _analyticsExportAccessService;
        private readonly IAbpSession _abpSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsExportsController"/> class.
        /// </summary>
        public AnalyticsExportsController(
            IAnalyticsExportAccessService analyticsExportAccessService,
            IAbpSession abpSession)
        {
            _analyticsExportAccessService = analyticsExportAccessService;
            _abpSession = abpSession;
        }

        /// <summary>
        /// Downloads a stored analytics export for the current dataset owner.
        /// </summary>
        [HttpGet("{id:long}/download")]
        public async Task<IActionResult> DownloadAsync(long id)
        {
            try
            {
                var download = await _analyticsExportAccessService.OpenDownloadAsync(id, GetRequiredTenantId(), _abpSession.GetUserId());
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
                throw new UserFriendlyException("Analytics retrieval requires a tenant context.");
            }

            return _abpSession.TenantId.Value;
        }
    }
}
