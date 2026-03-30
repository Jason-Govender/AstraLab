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
    public class DatasetAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetAppService _datasetAppService;

        public DatasetAppService_Tests()
        {
            _datasetAppService = Resolve<IDatasetAppService>();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Dataset_With_Server_Side_Metadata()
        {
            var currentUserId = AbpSession.GetUserId();

            var output = await _datasetAppService.CreateAsync(new CreateDatasetDto
            {
                Name = "sales-dataset",
                Description = "Sales dataset for ingestion",
                SourceFormat = DatasetFormat.Csv,
                OriginalFileName = "sales.csv"
            });

            output.Name.ShouldBe("sales-dataset");
            output.Description.ShouldBe("Sales dataset for ingestion");
            output.SourceFormat.ShouldBe(DatasetFormat.Csv);
            output.Status.ShouldBe(DatasetStatus.Uploaded);
            output.OwnerUserId.ShouldBe(currentUserId);
            output.OriginalFileName.ShouldBe("sales.csv");

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == output.Id);
                dataset.TenantId.ShouldBe(AbpSession.GetTenantId());
                dataset.OwnerUserId.ShouldBe(currentUserId);
                dataset.Status.ShouldBe(DatasetStatus.Uploaded);
                dataset.OriginalFileName.ShouldBe("sales.csv");
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GetAsync_Should_Return_Dataset_For_Current_Tenant()
        {
            var datasetId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "tenant-dataset",
                    Description = "Tenant dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant.json"
                }).Entity;

                context.SaveChanges();
                return dataset.Id;
            });

            var output = await _datasetAppService.GetAsync(new EntityDto<long>(datasetId));

            output.Id.ShouldBe(datasetId);
            output.Name.ShouldBe("tenant-dataset");
            output.Status.ShouldBe(DatasetStatus.Ready);
            output.OriginalFileName.ShouldBe("tenant.json");
        }

        [Fact]
        public async Task GetAsync_Should_Hide_Dataset_From_Other_Tenant()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("datasets-other", "Datasets Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantDatasetId = UsingDbContext(otherTenantId, context =>
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
                return dataset.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetAppService.GetAsync(new EntityDto<long>(otherTenantDatasetId)));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Current_Tenant_Datasets_And_Order_Newest_First()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("datasets-list-other", "Datasets List Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            UsingDbContext(1, context =>
            {
                context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "older-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "older.csv",
                    CreationTime = new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc)
                });

                context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "newer-dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "newer.json",
                    CreationTime = new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc)
                });
            });

            UsingDbContext(otherTenantId, context =>
            {
                context.Datasets.Add(new Dataset
                {
                    TenantId = otherTenantId,
                    Name = "other-tenant-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = 1001,
                    OriginalFileName = "other-tenant.csv",
                    CreationTime = new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc)
                });
            });

            var output = await _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0
            });

            output.TotalCount.ShouldBe(2);
            output.Items.Count.ShouldBe(2);
            output.Items[0].Name.ShouldBe("newer-dataset");
            output.Items[1].Name.ShouldBe("older-dataset");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Keyword_And_Status()
        {
            UsingDbContext(1, context =>
            {
                context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "sales-raw",
                    Description = "Quarterly sales extract",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "sales-raw.csv"
                });

                context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "finance-ready",
                    Description = "Finance dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "finance-ready.json"
                });
            });

            var keywordOutput = await _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Keyword = "sales"
            });

            keywordOutput.TotalCount.ShouldBe(1);
            keywordOutput.Items.Single().Name.ShouldBe("sales-raw");

            var statusOutput = await _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Status = DatasetStatus.Ready
            });

            statusOutput.TotalCount.ShouldBe(1);
            statusOutput.Items.Single().Name.ShouldBe("finance-ready");
        }

        [Fact]
        public async Task Host_Context_Should_Be_Rejected_For_Dataset_Operations()
        {
            LoginAsHostAdmin();

            var createException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.CreateAsync(new CreateDatasetDto
                {
                    Name = "host-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OriginalFileName = "host.csv"
                }));

            createException.Message.ShouldBe("Tenant context is required for dataset operations.");

            var getException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.GetAsync(new EntityDto<long>(1)));

            getException.Message.ShouldBe("Tenant context is required for dataset operations.");

            var listException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
                {
                    MaxResultCount = 20,
                    SkipCount = 0
                }));

            listException.Message.ShouldBe("Tenant context is required for dataset operations.");
        }
    }
}
