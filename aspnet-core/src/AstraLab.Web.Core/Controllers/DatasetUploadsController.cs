using System.IO;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Models.Datasets;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AstraLab.Controllers
{
    /// <summary>
    /// Provides the multipart HTTP boundary for validated raw dataset uploads.
    /// </summary>
    [ApiController]
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    [Route("api/services/app/datasets")]
    public class DatasetUploadsController : AstraLabControllerBase
    {
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetUploadsController"/> class.
        /// </summary>
        public DatasetUploadsController(IDatasetIngestionAppService datasetIngestionAppService)
        {
            _datasetIngestionAppService = datasetIngestionAppService;
        }

        /// <summary>
        /// Uploads a raw dataset file through the validated ingestion boundary.
        /// </summary>
        [HttpPost("upload-raw")]
        public async Task<IActionResult> UploadRawAsync([FromForm] UploadRawDatasetRequestModel model)
        {
            try
            {
                if (model?.File == null)
                {
                    return BadRequest(new { message = "A dataset file is required." });
                }

                using (var memoryStream = new MemoryStream())
                {
                    await model.File.CopyToAsync(memoryStream);

                    var result = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                    {
                        Name = model.Name,
                        Description = model.Description,
                        OriginalFileName = model.File.FileName,
                        ContentType = model.File.ContentType,
                        Content = memoryStream.ToArray()
                    });

                    return Ok(result);
                }
            }
            catch (UserFriendlyException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
