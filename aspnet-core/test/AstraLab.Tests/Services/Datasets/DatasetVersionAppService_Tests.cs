using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetVersionAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetVersionAppService _datasetVersionAppService;

        public DatasetVersionAppService_Tests()
        {
            _datasetVersionAppService = Resolve<IDatasetVersionAppService>();
        }

        [Fact]
        public async Task CreateAsync_Should_Create_First_Raw_Version_And_Promote_It_To_Current()
        {
            var datasetId = await CreateDatasetAsync();

            var output = await _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
            {
                DatasetId = datasetId,
                VersionType = DatasetVersionType.Raw,
                RowCount = 125,
                ColumnCount = 8,
                SchemaJson = "{\"columns\":8}",
                SizeBytes = 2048
            });

            output.DatasetId.ShouldBe(datasetId);
            output.VersionNumber.ShouldBe(1);
            output.VersionType.ShouldBe(DatasetVersionType.Raw);
            output.Status.ShouldBe(DatasetVersionStatus.Active);
            output.ParentVersionId.ShouldBeNull();
            output.RowCount.ShouldBe(125);
            output.ColumnCount.ShouldBe(8);
            output.SchemaJson.ShouldBe("{\"columns\":8}");
            output.SizeBytes.ShouldBe(2048);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == datasetId);
                var version = context.DatasetVersions.Single(item => item.Id == output.Id);

                dataset.CurrentVersionId.ShouldBe(output.Id);
                version.TenantId.ShouldBe(AbpSession.GetTenantId());
                version.Status.ShouldBe(DatasetVersionStatus.Active);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task CreateAsync_Should_Increment_Version_Number_And_Supersede_Previous_Current_Version()
        {
            var datasetId = await CreateDatasetAsync();

            var rawVersion = await _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
            {
                DatasetId = datasetId,
                VersionType = DatasetVersionType.Raw,
                SizeBytes = 1024
            });

            var processedVersion = await _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
            {
                DatasetId = datasetId,
                VersionType = DatasetVersionType.Processed,
                ParentVersionId = rawVersion.Id,
                RowCount = 100,
                ColumnCount = 6,
                SizeBytes = 768
            });

            processedVersion.VersionNumber.ShouldBe(2);
            processedVersion.Status.ShouldBe(DatasetVersionStatus.Active);
            processedVersion.ParentVersionId.ShouldBe(rawVersion.Id);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == datasetId);
                var rawEntity = context.DatasetVersions.Single(item => item.Id == rawVersion.Id);
                var processedEntity = context.DatasetVersions.Single(item => item.Id == processedVersion.Id);

                dataset.CurrentVersionId.ShouldBe(processedVersion.Id);
                rawEntity.Status.ShouldBe(DatasetVersionStatus.Superseded);
                processedEntity.Status.ShouldBe(DatasetVersionStatus.Active);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task CreateAsync_Should_Reject_Processed_Version_Without_Parent()
        {
            var datasetId = await CreateDatasetAsync();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
                {
                    DatasetId = datasetId,
                    VersionType = DatasetVersionType.Processed,
                    SizeBytes = 400
                }));

            exception.Message.ShouldBe("Processed dataset versions must reference a parent version.");
        }

        [Fact]
        public async Task CreateAsync_Should_Reject_Raw_Version_With_Parent()
        {
            var datasetId = await CreateDatasetAsync();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
                {
                    DatasetId = datasetId,
                    VersionType = DatasetVersionType.Raw,
                    ParentVersionId = 99,
                    SizeBytes = 400
                }));

            exception.Message.ShouldBe("Raw dataset versions cannot reference a parent version.");
        }

        [Fact]
        public async Task CreateAsync_Should_Reject_Parent_Version_From_Another_Dataset()
        {
            var firstDatasetId = await CreateDatasetAsync("first-dataset", "first.csv");
            var secondDatasetId = await CreateDatasetAsync("second-dataset", "second.csv");

            var firstDatasetVersion = await _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
            {
                DatasetId = firstDatasetId,
                VersionType = DatasetVersionType.Raw,
                SizeBytes = 200
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
                {
                    DatasetId = secondDatasetId,
                    VersionType = DatasetVersionType.Processed,
                    ParentVersionId = firstDatasetVersion.Id,
                    SizeBytes = 300
                }));

            exception.Message.ShouldBe("Parent dataset version was not found for the specified dataset.");
        }

        [Fact]
        public async Task GetAsync_Should_Return_Dataset_Version_For_Current_Tenant()
        {
            var datasetId = await CreateDatasetAsync();
            var datasetVersion = await _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
            {
                DatasetId = datasetId,
                VersionType = DatasetVersionType.Raw,
                SizeBytes = 512
            });

            var output = await _datasetVersionAppService.GetAsync(new EntityDto<long>(datasetVersion.Id));

            output.Id.ShouldBe(datasetVersion.Id);
            output.DatasetId.ShouldBe(datasetId);
            output.VersionNumber.ShouldBe(1);
            output.Status.ShouldBe(DatasetVersionStatus.Active);
        }

        [Fact]
        public async Task GetAsync_Should_Hide_Dataset_Version_From_Other_Tenant()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("dataset-version-other", "Dataset Version Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantVersionId = UsingDbContext(otherTenantId, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = otherTenantId,
                    Name = "other-tenant-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = 999,
                    OriginalFileName = "other.csv"
                }).Entity;

                context.SaveChanges();

                var version = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = otherTenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 100
                }).Entity;

                context.SaveChanges();
                return version.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetVersionAppService.GetAsync(new EntityDto<long>(otherTenantVersionId)));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Dataset_Scoped_Versions_Ordered_Newest_First_And_Filter_By_Status()
        {
            var datasetId = await CreateDatasetAsync();
            var otherDatasetId = await CreateDatasetAsync("another-dataset", "another.csv");

            UsingDbContext(1, context =>
            {
                context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = datasetId,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Superseded,
                    SizeBytes = 100,
                    CreationTime = new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc)
                });

                context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = datasetId,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 80,
                    CreationTime = new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc)
                });

                context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = otherDatasetId,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 60,
                    CreationTime = new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc)
                });
            });

            var output = await _datasetVersionAppService.GetAllAsync(new PagedDatasetVersionResultRequestDto
            {
                DatasetId = datasetId,
                MaxResultCount = 20,
                SkipCount = 0
            });

            output.TotalCount.ShouldBe(2);
            output.Items.Count.ShouldBe(2);
            output.Items[0].VersionNumber.ShouldBe(2);
            output.Items[1].VersionNumber.ShouldBe(1);

            var filteredOutput = await _datasetVersionAppService.GetAllAsync(new PagedDatasetVersionResultRequestDto
            {
                DatasetId = datasetId,
                MaxResultCount = 20,
                SkipCount = 0,
                Status = DatasetVersionStatus.Active
            });

            filteredOutput.TotalCount.ShouldBe(1);
            filteredOutput.Items.Single().VersionNumber.ShouldBe(2);
        }

        [Fact]
        public async Task Host_Context_Should_Be_Rejected_For_Dataset_Version_Operations()
        {
            LoginAsHostAdmin();

            var createException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.CreateAsync(new CreateDatasetVersionDto
                {
                    DatasetId = 1,
                    VersionType = DatasetVersionType.Raw,
                    SizeBytes = 100
                }));

            createException.Message.ShouldBe("Tenant context is required for dataset version operations.");

            var getException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.GetAsync(new EntityDto<long>(1)));

            getException.Message.ShouldBe("Tenant context is required for dataset version operations.");

            var listException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetVersionAppService.GetAllAsync(new PagedDatasetVersionResultRequestDto
                {
                    DatasetId = 1,
                    MaxResultCount = 20,
                    SkipCount = 0
                }));

            listException.Message.ShouldBe("Tenant context is required for dataset version operations.");
        }

        private async Task<long> CreateDatasetAsync(string name = "dataset-for-versioning", string originalFileName = "dataset.csv")
        {
            return await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = name,
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = originalFileName
                }).Entity;

                await context.SaveChangesAsync();
                return dataset.Id;
            });
        }
    }
}
