using System.IO;
using System.Text;
using System.Threading.Tasks;
using AstraLab.Controllers;
using AstraLab.Models.Datasets;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Controllers
{
    public class DatasetUploadsController_Tests : AstraLabTestBase
    {
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;

        public DatasetUploadsController_Tests()
        {
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
        }

        [Fact]
        public async Task UploadRawAsync_Should_Return_Ok_For_A_Valid_Csv_File()
        {
            var controller = new DatasetUploadsController(_datasetIngestionAppService);

            var result = await controller.UploadRawAsync(new UploadRawDatasetRequestModel
            {
                Name = "Controller CSV",
                File = CreateFormFile("controller.csv", "text/csv", "id,name\n1,Alice\n")
            });

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var payload = okResult.Value.ShouldBeOfType<UploadedRawDatasetDto>();

            payload.ColumnCount.ShouldBe(2);
            payload.Columns.Count.ShouldBe(2);
            payload.SchemaJson.ShouldContain("\"format\":\"csv\"");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Return_BadRequest_For_An_Unsupported_File()
        {
            var controller = new DatasetUploadsController(_datasetIngestionAppService);

            var result = await controller.UploadRawAsync(new UploadRawDatasetRequestModel
            {
                Name = "Controller XLSX",
                File = CreateFormFile("controller.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "fake")
            });

            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UploadRawAsync_Should_Return_BadRequest_When_File_Is_Missing()
        {
            var controller = new DatasetUploadsController(_datasetIngestionAppService);

            var result = await controller.UploadRawAsync(new UploadRawDatasetRequestModel
            {
                Name = "Missing File"
            });

            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        private static IFormFile CreateFormFile(string fileName, string contentType, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
