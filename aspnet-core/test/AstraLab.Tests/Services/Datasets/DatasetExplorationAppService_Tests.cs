using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Storage;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetExplorationAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetExplorationAppService _datasetExplorationAppService;
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;
        private readonly IRawDatasetStorage _rawDatasetStorage;

        public DatasetExplorationAppService_Tests()
        {
            _datasetExplorationAppService = Resolve<IDatasetExplorationAppService>();
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
            _rawDatasetStorage = Resolve<IRawDatasetStorage>();
        }

        [Fact]
        public async Task GetRowsAsync_Should_Return_Paged_Csv_Rows_With_Total_Count()
        {
            var upload = await UploadCsvAsync(
                "Rows CSV",
                "id,amount,category,score\n1,10,A,1\n2,20,B,2\n3,30,A,3\n4,,C,4\n5,50,B,5\n");

            var output = await _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
            {
                DatasetVersionId = upload.DatasetVersionId,
                SkipCount = 1,
                MaxResultCount = 2
            });

            output.TotalCount.ShouldBe(5);
            output.Items.Count.ShouldBe(2);
            output.Items.Select(item => item.RowNumber).ShouldBe(new[] { 2, 3 });
            output.Items[0].Values.ShouldBe(new[] { "2", "20", "B", "2" });
            output.Items[1].Values.ShouldBe(new[] { "3", "30", "A", "3" });
        }

        [Fact]
        public async Task GetRowsAsync_Should_Return_Paged_Tabular_Json_Rows()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Rows JSON",
                OriginalFileName = "rows.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("[{\"id\":1,\"name\":\"Alice\"},{\"id\":2,\"name\":\"Bob\"},{\"id\":3,\"name\":\"Carla\"}]")
            });

            var output = await _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
            {
                DatasetVersionId = upload.DatasetVersionId,
                SkipCount = 1,
                MaxResultCount = 1
            });

            output.TotalCount.ShouldBe(3);
            output.Items.Count.ShouldBe(1);
            output.Items.Single().RowNumber.ShouldBe(2);
            output.Items.Single().Values.ShouldBe(new[] { "2", "Bob" });
        }

        [Fact]
        public async Task GetRowsAsync_Should_Reject_NonTabular_Json()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Scalar JSON",
                OriginalFileName = "scalar.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("\"hello\"")
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
                {
                    DatasetVersionId = upload.DatasetVersionId,
                    SkipCount = 0,
                    MaxResultCount = 10
                }));

            exception.Message.ShouldBe("Only tabular JSON datasets can be explored.");
        }

        [Fact]
        public async Task GetColumnsAsync_Should_Return_Chart_Eligibility_Metadata()
        {
            var datasetVersionId = await CreateStoredDatasetVersionAsync(
                1,
                AbpSession.UserId.Value,
                DatasetFormat.Csv,
                "columns.csv",
                "text/csv",
                "id,amount,category,createdAt\n1,10,A,2024-01-01T00:00:00Z\n2,20,B,2024-01-02T00:00:00Z\n",
                new SeedColumn("id", "integer", 1),
                new SeedColumn("amount", "integer", 2),
                new SeedColumn("category", "string", 3),
                new SeedColumn("createdAt", "datetime", 4));

            var output = await _datasetExplorationAppService.GetColumnsAsync(new EntityDto<long>(datasetVersionId));

            output.DatasetVersionId.ShouldBe(datasetVersionId);
            output.Columns.Count.ShouldBe(4);

            output.Columns.Single(item => item.Name == "id").IsNumeric.ShouldBeTrue();
            output.Columns.Single(item => item.Name == "amount").DataType.ShouldBe("integer");
            output.Columns.Single(item => item.Name == "category").IsCategorical.ShouldBeTrue();
            output.Columns.Single(item => item.Name == "createdAt").IsTemporal.ShouldBeTrue();
        }

        [Fact]
        public async Task GetHistogramAsync_Should_Return_Numeric_Buckets()
        {
            var upload = await UploadCsvAsync(
                "Histogram CSV",
                "id,amount\n1,10\n2,20\n3,30\n4,\n5,50\n");
            var amountColumnId = upload.Columns.Single(item => item.Name == "amount").Id;

            var output = await _datasetExplorationAppService.GetHistogramAsync(new GetHistogramChartRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                DatasetColumnId = amountColumnId,
                BucketCount = 5
            });

            output.DatasetColumnId.ShouldBe(amountColumnId);
            output.ValueCount.ShouldBe(4);
            output.NullCount.ShouldBe(1);
            output.Min.ShouldBe(10m);
            output.Max.ShouldBe(50m);
            output.Buckets.Sum(item => item.Count).ShouldBe(4);
        }

        [Fact]
        public async Task GetBarChartAsync_Should_Return_Categorical_Frequencies()
        {
            var upload = await UploadCsvAsync(
                "Bar CSV",
                "id,category\n1,A\n2,B\n3,A\n4,C\n5,B\n");
            var categoryColumnId = upload.Columns.Single(item => item.Name == "category").Id;

            var output = await _datasetExplorationAppService.GetBarChartAsync(new GetBarChartRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                DatasetColumnId = categoryColumnId,
                TopCategoryCount = 2
            });

            output.DistinctCategoryCount.ShouldBe(3);
            output.NullCount.ShouldBe(0);
            output.Categories.Count.ShouldBe(2);
            output.Categories[0].Count.ShouldBe(2);
            output.Categories.Select(item => item.Label).ShouldBe(new[] { "A", "B" });
        }

        [Fact]
        public async Task GetScatterPlotAsync_Should_Return_Numeric_Points()
        {
            var upload = await UploadCsvAsync(
                "Scatter CSV",
                "amount,score\n10,1\n20,2\n30,3\n,4\n50,5\n");
            var amountColumnId = upload.Columns.Single(item => item.Name == "amount").Id;
            var scoreColumnId = upload.Columns.Single(item => item.Name == "score").Id;

            var output = await _datasetExplorationAppService.GetScatterPlotAsync(new GetScatterPlotRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                XDatasetColumnId = amountColumnId,
                YDatasetColumnId = scoreColumnId,
                MaxPointCount = 10
            });

            output.PointCount.ShouldBe(4);
            output.Points[0].RowNumber.ShouldBe(1);
            output.Points[0].X.ShouldBe(10m);
            output.Points[0].Y.ShouldBe(1m);
            output.Points.Any(item => item.RowNumber == 4).ShouldBeFalse();
        }

        [Fact]
        public async Task GetDistributionAsync_Should_Return_Numeric_And_Categorical_Distributions()
        {
            var upload = await UploadCsvAsync(
                "Distribution CSV",
                "amount,category\n10,A\n20,B\n30,A\n, C\n50,B\n".Replace(" C", "C", StringComparison.Ordinal));
            var amountColumnId = upload.Columns.Single(item => item.Name == "amount").Id;
            var categoryColumnId = upload.Columns.Single(item => item.Name == "category").Id;

            var numericOutput = await _datasetExplorationAppService.GetDistributionAsync(new GetDistributionAnalysisRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                DatasetColumnId = amountColumnId,
                BucketCount = 4
            });

            numericOutput.ValueCount.ShouldBe(4);
            numericOutput.NullCount.ShouldBe(1);
            numericOutput.Mean.ShouldBe(27.5m);
            numericOutput.Median.ShouldBe(25m);
            numericOutput.Buckets.Sum(item => item.Count).ShouldBe(4);

            var categoricalOutput = await _datasetExplorationAppService.GetDistributionAsync(new GetDistributionAnalysisRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                DatasetColumnId = categoryColumnId,
                TopCategoryCount = 5
            });

            categoricalOutput.ValueCount.ShouldBe(5);
            categoricalOutput.NullCount.ShouldBe(0);
            categoricalOutput.DistinctCount.ShouldBe(3);
            categoricalOutput.Categories.Count.ShouldBe(3);
        }

        [Fact]
        public async Task GetCorrelationAsync_Should_Return_Pearson_Correlation_Pairs()
        {
            var upload = await UploadCsvAsync(
                "Correlation CSV",
                "x,y,z\n1,2,5\n2,4,4\n3,6,3\n4,8,2\n5,10,1\n");

            var xColumnId = upload.Columns.Single(item => item.Name == "x").Id;
            var yColumnId = upload.Columns.Single(item => item.Name == "y").Id;
            var zColumnId = upload.Columns.Single(item => item.Name == "z").Id;

            var output = await _datasetExplorationAppService.GetCorrelationAsync(new GetCorrelationAnalysisRequest
            {
                DatasetVersionId = upload.DatasetVersionId,
                DatasetColumnIds = new List<long> { xColumnId, yColumnId, zColumnId }
            });

            output.Columns.Count.ShouldBe(3);
            output.Pairs.Count.ShouldBe(3);
            output.Pairs.Single(item => item.XColumnName == "x" && item.YColumnName == "y").Coefficient.ShouldBe(1m);
            output.Pairs.Single(item => item.XColumnName == "x" && item.YColumnName == "z").Coefficient.ShouldBe(-1m);
        }

        [Fact]
        public async Task Chart_Endpoints_Should_Reject_Invalid_Column_Type_Combinations()
        {
            var datasetVersionId = await CreateStoredDatasetVersionAsync(
                1,
                AbpSession.UserId.Value,
                DatasetFormat.Csv,
                "invalid-chart.csv",
                "text/csv",
                "id,category,createdAt\n1,A,2024-01-01T00:00:00Z\n2,B,2024-01-02T00:00:00Z\n",
                new SeedColumn("id", "integer", 1),
                new SeedColumn("category", "string", 2),
                new SeedColumn("createdAt", "datetime", 3));

            var columns = UsingDbContext(context => context.DatasetColumns
                .Where(item => item.DatasetVersionId == datasetVersionId)
                .OrderBy(item => item.Ordinal)
                .ToList());

            var idColumnId = columns.Single(item => item.Name == "id").Id;
            var categoryColumnId = columns.Single(item => item.Name == "category").Id;
            var createdAtColumnId = columns.Single(item => item.Name == "createdAt").Id;

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetHistogramAsync(new GetHistogramChartRequest
                {
                    DatasetVersionId = datasetVersionId,
                    DatasetColumnId = categoryColumnId
                }))).Message.ShouldBe("Histograms are only supported for numeric columns.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetBarChartAsync(new GetBarChartRequest
                {
                    DatasetVersionId = datasetVersionId,
                    DatasetColumnId = idColumnId
                }))).Message.ShouldBe("Bar charts are only supported for categorical columns.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetScatterPlotAsync(new GetScatterPlotRequest
                {
                    DatasetVersionId = datasetVersionId,
                    XDatasetColumnId = idColumnId,
                    YDatasetColumnId = categoryColumnId
                }))).Message.ShouldBe("Scatter plots require two numeric columns.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetDistributionAsync(new GetDistributionAnalysisRequest
                {
                    DatasetVersionId = datasetVersionId,
                    DatasetColumnId = createdAtColumnId
                }))).Message.ShouldBe("Distribution analysis is only supported for numeric and categorical columns.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetCorrelationAsync(new GetCorrelationAnalysisRequest
                {
                    DatasetVersionId = datasetVersionId,
                    DatasetColumnIds = new List<long> { idColumnId, categoryColumnId }
                }))).Message.ShouldBe("Correlation analysis is only supported for numeric columns.");
        }

        [Fact]
        public async Task Exploration_Endpoints_Should_Reject_Cross_Owner_Cross_Tenant_And_Host_Context()
        {
            var upload = await UploadCsvAsync(
                "Owner CSV",
                "id,value\n1,10\n2,20\n");
            var valueColumnId = upload.Columns.Single(item => item.Name == "value").Id;

            UsingDbContext(context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.OwnerUserId = dataset.OwnerUserId + 77;
                context.SaveChanges();
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
                {
                    DatasetVersionId = upload.DatasetVersionId,
                    SkipCount = 0,
                    MaxResultCount = 10
                }));

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetExplorationAppService.GetColumnsAsync(new EntityDto<long>(upload.DatasetVersionId)));

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetExplorationAppService.GetHistogramAsync(new GetHistogramChartRequest
                {
                    DatasetVersionId = upload.DatasetVersionId,
                    DatasetColumnId = valueColumnId
                }));

            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("explore-other", "Explore Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantVersionId = await CreateStoredDatasetVersionAsync(
                otherTenantId,
                999,
                DatasetFormat.Csv,
                "other.csv",
                "text/csv",
                "id,value\n1,10\n2,20\n",
                new SeedColumn("id", "integer", 1),
                new SeedColumn("value", "integer", 2));

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
                {
                    DatasetVersionId = otherTenantVersionId,
                    SkipCount = 0,
                    MaxResultCount = 10
                }));

            LoginAsHostAdmin();

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetRowsAsync(new PagedDatasetRowRequestDto
                {
                    DatasetVersionId = 1,
                    SkipCount = 0,
                    MaxResultCount = 10
                }))).Message.ShouldBe("Tenant context is required for dataset exploration operations.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetColumnsAsync(new EntityDto<long>(1)))).Message.ShouldBe("Tenant context is required for dataset exploration operations.");

            (await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetExplorationAppService.GetHistogramAsync(new GetHistogramChartRequest
                {
                    DatasetVersionId = 1,
                    DatasetColumnId = 1
                }))).Message.ShouldBe("Tenant context is required for dataset exploration operations.");
        }

        private async Task<UploadedRawDatasetDto> UploadCsvAsync(string name, string content)
        {
            return await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = name,
                OriginalFileName = $"{name.Replace(" ", "-", StringComparison.OrdinalIgnoreCase)}.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes(content)
            });
        }

        private async Task<long> CreateStoredDatasetVersionAsync(
            int tenantId,
            long ownerUserId,
            DatasetFormat datasetFormat,
            string originalFileName,
            string contentType,
            string content,
            params SeedColumn[] columns)
        {
            var seed = UsingDbContext(tenantId, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = tenantId,
                    Name = $"seed-{Guid.NewGuid():N}",
                    SourceFormat = datasetFormat,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = ownerUserId,
                    OriginalFileName = originalFileName
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = tenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = Encoding.UTF8.GetByteCount(content),
                    ColumnCount = columns.Length
                }).Entity;

                context.SaveChanges();

                foreach (var column in columns)
                {
                    context.DatasetColumns.Add(new DatasetColumn
                    {
                        TenantId = tenantId,
                        DatasetVersionId = datasetVersion.Id,
                        Name = column.Name,
                        DataType = column.DataType,
                        IsDataTypeInferred = false,
                        Ordinal = column.Ordinal
                    });
                }

                dataset.CurrentVersionId = datasetVersion.Id;
                context.SaveChanges();

                return new SeedVersionIds
                {
                    DatasetId = dataset.Id,
                    DatasetVersionId = datasetVersion.Id
                };
            });

            StoredRawDatasetFileResult storedFile;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                storedFile = await _rawDatasetStorage.StoreAsync(new StoreRawDatasetFileRequest
                {
                    TenantId = tenantId,
                    DatasetId = seed.DatasetId,
                    DatasetVersionId = seed.DatasetVersionId,
                    OriginalFileName = originalFileName,
                    ContentType = contentType,
                    Content = stream
                });
            }

            UsingDbContext(tenantId, context =>
            {
                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = tenantId,
                    DatasetVersionId = seed.DatasetVersionId,
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey,
                    OriginalFileName = storedFile.OriginalFileName,
                    ContentType = contentType,
                    SizeBytes = storedFile.SizeBytes,
                    ChecksumSha256 = storedFile.ChecksumSha256
                });

                context.SaveChanges();
            });

            return seed.DatasetVersionId;
        }

        private class SeedVersionIds
        {
            public long DatasetId { get; set; }

            public long DatasetVersionId { get; set; }
        }

        private class SeedColumn
        {
            public SeedColumn(string name, string dataType, int ordinal)
            {
                Name = name;
                DataType = dataType;
                Ordinal = ordinal;
            }

            public string Name { get; }

            public string DataType { get; }

            public int Ordinal { get; }
        }
    }
}
