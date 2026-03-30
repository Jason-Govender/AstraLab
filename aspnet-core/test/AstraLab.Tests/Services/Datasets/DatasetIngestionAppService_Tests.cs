using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetIngestionAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;

        public DatasetIngestionAppService_Tests()
        {
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
        }

        [Fact]
        public async Task UploadRawAsync_Should_Create_Dataset_Version_And_File_For_Valid_Csv()
        {
            var result = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Customers CSV",
                Description = "csv upload",
                OriginalFileName = "customers.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,name\n1,Alice\n2,Bob\n")
            });

            result.Dataset.Name.ShouldBe("Customers CSV");
            result.Dataset.SourceFormat.ShouldBe(DatasetFormat.Csv);
            result.StorageProvider.ShouldBe("local-filesystem");
            result.StorageKey.ShouldContain("/raw/");
            result.ColumnCount.ShouldBe(2);
            result.Columns.Count.ShouldBe(2);
            result.Columns[0].Name.ShouldBe("id");
            result.Columns[1].Name.ShouldBe("name");
            result.SchemaJson.ShouldContain("\"format\":\"csv\"");
            result.SchemaJson.ShouldContain("\"rootKind\":\"tabular\"");

            await UsingDbContextAsync(async context =>
            {
                context.Datasets.Count().ShouldBe(1);
                context.DatasetVersions.Count().ShouldBe(1);
                context.DatasetFiles.Count().ShouldBe(1);
                context.DatasetColumns.Count().ShouldBe(2);

                var dataset = context.Datasets.Single();
                var datasetVersion = context.DatasetVersions.Single();
                var datasetFile = context.DatasetFiles.Single();
                var datasetColumns = context.DatasetColumns.OrderBy(item => item.Ordinal).ToList();

                dataset.CurrentVersionId.ShouldBe(datasetVersion.Id);
                datasetVersion.VersionType.ShouldBe(DatasetVersionType.Raw);
                datasetVersion.ColumnCount.ShouldBe(2);
                datasetVersion.SchemaJson.ShouldContain("\"columns\"");
                datasetFile.DatasetVersionId.ShouldBe(datasetVersion.Id);
                datasetColumns[0].Name.ShouldBe("id");
                datasetColumns[0].DataType.ShouldBe("string");
                datasetColumns[1].Name.ShouldBe("name");

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task UploadRawAsync_Should_Create_Dataset_Version_File_And_Columns_For_Valid_Json()
        {
            var result = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Customers JSON",
                OriginalFileName = "customers.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("[{\"id\":1,\"name\":\"Alice\"},{\"id\":2,\"isActive\":true}]")
            });

            result.Dataset.SourceFormat.ShouldBe(DatasetFormat.Json);
            result.StorageKey.ShouldEndWith(".json");
            result.ColumnCount.ShouldBe(3);
            result.Columns.Select(item => item.Name).ShouldBe(new[] { "id", "name", "isActive" });
            result.Columns.Select(item => item.DataType).ShouldBe(new[] { "integer", "string", "boolean" });
            result.SchemaJson.ShouldContain("\"format\":\"json\"");
            result.SchemaJson.ShouldContain("\"rootKind\":\"array\"");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Succeed_For_Valid_NonTabular_Json_And_Persist_Zero_Columns()
        {
            var result = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Scalar JSON",
                OriginalFileName = "scalar.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("\"hello\"")
            });

            result.Dataset.SourceFormat.ShouldBe(DatasetFormat.Json);
            result.ColumnCount.ShouldBe(0);
            result.Columns.ShouldBeEmpty();
            result.SchemaJson.ShouldContain("\"rootKind\":\"scalar\"");

            await UsingDbContextAsync(async context =>
            {
                context.DatasetColumns.Count().ShouldBe(0);

                var datasetVersion = context.DatasetVersions.Single();
                datasetVersion.ColumnCount.ShouldBe(0);
                datasetVersion.SchemaJson.ShouldContain("\"rootKind\":\"scalar\"");

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task UploadRawAsync_Should_Reject_Unsupported_Extensions()
        {
            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Spreadsheet",
                    OriginalFileName = "spreadsheet.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Content = Encoding.UTF8.GetBytes("fake")
                }));

            exception.Message.ShouldBe("Only CSV and JSON dataset files are supported.");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Reject_Empty_Files()
        {
            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Empty CSV",
                    OriginalFileName = "empty.csv",
                    ContentType = "text/csv",
                    Content = new byte[0]
                }));

            exception.Message.ShouldBe("Uploaded dataset files cannot be empty.");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Reject_Malformed_Json()
        {
            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Broken JSON",
                    OriginalFileName = "broken.json",
                    ContentType = "application/json",
                    Content = Encoding.UTF8.GetBytes("{ invalid json }")
                }));

            exception.Message.ShouldBe("The uploaded JSON file is malformed.");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Reject_Malformed_Csv()
        {
            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Broken CSV",
                    OriginalFileName = "broken.csv",
                    ContentType = "text/csv",
                    Content = Encoding.UTF8.GetBytes("id,name\n1\n")
                }));

            exception.Message.ShouldBe("The uploaded CSV file is malformed.");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Reject_Host_Context()
        {
            LoginAsHostAdmin();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Host Upload",
                    OriginalFileName = "host.csv",
                    ContentType = "text/csv",
                    Content = Encoding.UTF8.GetBytes("id,name\n1,Alice\n")
                }));

            exception.Message.ShouldBe("Tenant context is required for dataset upload operations.");
        }

        [Fact]
        public async Task UploadRawAsync_Should_Not_Persist_Partial_Records_When_Validation_Fails()
        {
            var initialCounts = await UsingDbContextAsync(async context =>
            {
                return new
                {
                    DatasetCount = context.Datasets.Count(),
                    DatasetVersionCount = context.DatasetVersions.Count(),
                    DatasetFileCount = context.DatasetFiles.Count(),
                    DatasetColumnCount = context.DatasetColumns.Count()
                };
            });

            await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Broken JSON",
                    OriginalFileName = "broken.json",
                    ContentType = "application/json",
                    Content = Encoding.UTF8.GetBytes("{ invalid json }")
                }));

            await UsingDbContextAsync(async context =>
            {
                context.Datasets.Count().ShouldBe(initialCounts.DatasetCount);
                context.DatasetVersions.Count().ShouldBe(initialCounts.DatasetVersionCount);
                context.DatasetFiles.Count().ShouldBe(initialCounts.DatasetFileCount);
                context.DatasetColumns.Count().ShouldBe(initialCounts.DatasetColumnCount);

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task UploadRawAsync_Should_Not_Persist_Partial_Records_When_Metadata_Extraction_Fails()
        {
            var initialCounts = await UsingDbContextAsync(async context =>
            {
                return new
                {
                    DatasetCount = context.Datasets.Count(),
                    DatasetVersionCount = context.DatasetVersions.Count(),
                    DatasetFileCount = context.DatasetFiles.Count(),
                    DatasetColumnCount = context.DatasetColumns.Count()
                };
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
                {
                    Name = "Unnamed Header CSV",
                    OriginalFileName = "unnamed-header.csv",
                    ContentType = "text/csv",
                    Content = Encoding.UTF8.GetBytes(",name\n1,Alice\n")
                }));

            exception.Message.ShouldBe("The uploaded CSV file must contain named header columns.");

            await UsingDbContextAsync(async context =>
            {
                context.Datasets.Count().ShouldBe(initialCounts.DatasetCount);
                context.DatasetVersions.Count().ShouldBe(initialCounts.DatasetVersionCount);
                context.DatasetFiles.Count().ShouldBe(initialCounts.DatasetFileCount);
                context.DatasetColumns.Count().ShouldBe(initialCounts.DatasetColumnCount);

                await Task.CompletedTask;
            });
        }
    }
}
