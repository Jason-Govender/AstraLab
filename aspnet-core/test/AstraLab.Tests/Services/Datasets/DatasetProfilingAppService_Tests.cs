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
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetProfilingAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;
        private readonly IDatasetProfilingAppService _datasetProfilingAppService;

        public DatasetProfilingAppService_Tests()
        {
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
            _datasetProfilingAppService = Resolve<IDatasetProfilingAppService>();
        }

        [Fact]
        public async Task ProfileAsync_Should_Reprofile_DatasetVersion_And_Replace_Current_Profile_Snapshot()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Profile CSV",
                OriginalFileName = "profile.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,amount,name\n1,10.5,Alice\n2,,\n1,10.5,Alice\n")
            });

            var output = await _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(upload.DatasetVersionId));

            output.RowCount.ShouldBe(3);
            output.DuplicateRowCount.ShouldBe(1);
            output.ColumnProfiles.Count.ShouldBe(3);
            output.ColumnProfiles.Single(item => item.InferredDataType == "decimal").NullCount.ShouldBe(1);

            await UsingDbContextAsync(async context =>
            {
                context.DatasetProfiles.Count(item => item.DatasetVersionId == upload.DatasetVersionId).ShouldBe(1);
                context.DatasetColumnProfiles.Count().ShouldBe(3);

                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == upload.DatasetVersionId);
                var datasetColumns = context.DatasetColumns
                    .Where(item => item.DatasetVersionId == upload.DatasetVersionId)
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                dataset.Status.ShouldBe(DatasetStatus.Ready);
                datasetVersion.Status.ShouldBe(DatasetVersionStatus.Active);
                datasetVersion.RowCount.ShouldBe(3);
                datasetColumns[1].DataType.ShouldBe("decimal");
                datasetColumns[1].NullCount.ShouldBe(1);

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GetAsync_Should_Return_Typed_Profile_Summary_For_Profiled_Dataset_Version()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Summary CSV",
                OriginalFileName = "summary.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,amount,name\n1,10.5,Alice\n2,,\n1,10.5,Alice\n")
            });

            var output = await _datasetProfilingAppService.GetAsync(new EntityDto<long>(upload.DatasetVersionId));

            output.DatasetVersionId.ShouldBe(upload.DatasetVersionId);
            output.ProfileId.ShouldBeGreaterThan(0L);
            output.RowCount.ShouldBe(3);
            output.DuplicateRowCount.ShouldBe(1);
            output.DataHealthScore.ShouldBeGreaterThan(0m);
            output.TotalNullCount.ShouldBe(2);
            output.OverallNullPercentage.ShouldBeGreaterThan(0m);
            output.TotalAnomalyCount.ShouldBeGreaterThanOrEqualTo(0L);
            output.OverallAnomalyPercentage.ShouldBeGreaterThanOrEqualTo(0m);
        }

        [Fact]
        public async Task GetColumnsAsync_Should_Return_Typed_Column_Insights_Ordered_By_Ordinal()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Insights CSV",
                OriginalFileName = "insights.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,amount,name\n1,10.5,Alice\n2,,Bob\n1,999.9,Alice\n")
            });

            var output = await _datasetProfilingAppService.GetColumnsAsync(new PagedDatasetColumnInsightRequestDto
            {
                DatasetVersionId = upload.DatasetVersionId,
                MaxResultCount = 20,
                SkipCount = 0
            });

            output.TotalCount.ShouldBe(3);
            output.Items.Count.ShouldBe(3);
            output.Items.Select(item => item.Ordinal).ShouldBe(new[] { 1, 2, 3 });
            output.Items.Select(item => item.Name).ShouldBe(new[] { "id", "amount", "name" });

            var amountColumn = output.Items.Single(item => item.Name == "amount");
            amountColumn.InferredDataType.ShouldBe("decimal");
            amountColumn.NullCount.ShouldBe(1);
            amountColumn.NullPercentage.ShouldBeGreaterThan(0m);
            amountColumn.Mean.ShouldNotBeNull();
            amountColumn.Min.ShouldNotBeNull();
            amountColumn.Max.ShouldNotBeNull();
            amountColumn.AnomalyCount.ShouldBeGreaterThanOrEqualTo(0L);
        }

        [Fact]
        public async Task GetColumnsAsync_Should_Filter_By_Anomaly_And_Inferred_Type()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Anomaly CSV",
                OriginalFileName = "anomaly.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("amount,flag\n10,true\n10,false\n10,true\n10,false\n10,true\n10,false\n10,true\n10,false\n10,true\n1000,false\n")
            });

            var anomalyOutput = await _datasetProfilingAppService.GetColumnsAsync(new PagedDatasetColumnInsightRequestDto
            {
                DatasetVersionId = upload.DatasetVersionId,
                HasAnomalies = true,
                MaxResultCount = 20,
                SkipCount = 0
            });

            anomalyOutput.TotalCount.ShouldBe(1);
            anomalyOutput.Items.Single().Name.ShouldBe("amount");
            anomalyOutput.Items.Single().HasAnomalies.ShouldBeTrue();

            var typeOutput = await _datasetProfilingAppService.GetColumnsAsync(new PagedDatasetColumnInsightRequestDto
            {
                DatasetVersionId = upload.DatasetVersionId,
                InferredDataType = "boolean",
                MaxResultCount = 20,
                SkipCount = 0
            });

            typeOutput.TotalCount.ShouldBe(1);
            typeOutput.Items.Single().Name.ShouldBe("flag");
            typeOutput.Items.Single().InferredDataType.ShouldBe("boolean");
        }

        [Fact]
        public async Task GetAsync_Should_Throw_When_Profile_Does_Not_Exist_For_Dataset_Version()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "unprofiled-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.UserId.Value,
                    OriginalFileName = "unprofiled.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 25
                }).Entity;

                context.SaveChanges();
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetProfilingAppService.GetAsync(new EntityDto<long>(datasetVersionId)));
        }

        [Fact]
        public async Task GetColumnsAsync_Should_Hide_Dataset_Version_From_Other_Owner_In_Same_Tenant()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Other owner CSV",
                OriginalFileName = "other-owner.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,name\n1,Alice\n")
            });

            UsingDbContext(context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.OwnerUserId = dataset.OwnerUserId + 777;
                context.SaveChanges();
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetProfilingAppService.GetColumnsAsync(new PagedDatasetColumnInsightRequestDto
                {
                    DatasetVersionId = upload.DatasetVersionId,
                    MaxResultCount = 20,
                    SkipCount = 0
                }));
        }

        [Fact]
        public async Task GetAsync_Should_Hide_Dataset_Version_From_Other_Tenant()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("profiling-read-other", "Profiling Read Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantVersionId = UsingDbContext(otherTenantId, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = otherTenantId,
                    Name = "other-tenant-profiled-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = 999,
                    OriginalFileName = "other-tenant.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = otherTenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 64
                }).Entity;

                context.SaveChanges();

                context.DatasetProfiles.Add(new DatasetProfile
                {
                    TenantId = otherTenantId,
                    DatasetVersionId = datasetVersion.Id,
                    RowCount = 2,
                    DuplicateRowCount = 0,
                    DataHealthScore = 100m,
                    SummaryJson = "{\"totalNullCount\":0,\"overallNullPercentage\":0,\"totalAnomalyCount\":0,\"overallAnomalyPercentage\":0}"
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetProfilingAppService.GetAsync(new EntityDto<long>(otherTenantVersionId)));
        }

        [Fact]
        public async Task ProfileAsync_Should_Hide_Dataset_Version_From_Other_Owner_In_Same_Tenant()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Owner CSV",
                OriginalFileName = "owner.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,name\n1,Alice\n")
            });

            UsingDbContext(context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.OwnerUserId = dataset.OwnerUserId + 999;
                context.SaveChanges();
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(upload.DatasetVersionId)));
        }

        [Fact]
        public async Task Host_Context_Should_Be_Rejected_For_Dataset_Profiling_Operations()
        {
            LoginAsHostAdmin();

            var getException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetProfilingAppService.GetAsync(new EntityDto<long>(1)));

            getException.Message.ShouldBe("Tenant context is required for dataset profiling operations.");

            var listException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetProfilingAppService.GetColumnsAsync(new PagedDatasetColumnInsightRequestDto
                {
                    DatasetVersionId = 1,
                    MaxResultCount = 20,
                    SkipCount = 0
                }));

            listException.Message.ShouldBe("Tenant context is required for dataset profiling operations.");

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(1)));

            exception.Message.ShouldBe("Tenant context is required for dataset profiling operations.");
        }
    }
}
