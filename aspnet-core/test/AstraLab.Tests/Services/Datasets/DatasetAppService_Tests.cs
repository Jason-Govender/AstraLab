using System;
using System.Collections.Generic;
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
        public async Task GetAsync_Should_Hide_Dataset_From_Other_Owner_In_Same_Tenant()
        {
            var datasetId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "other-owner-dataset",
                    Description = "Other owner dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId() + 99,
                    OriginalFileName = "other-owner.csv"
                }).Entity;

                context.SaveChanges();
                return dataset.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetAppService.GetAsync(new EntityDto<long>(datasetId)));
        }

        [Fact]
        public async Task GetDetailsAsync_Should_Return_Dataset_Versions_Selected_Version_Columns_And_File_Metadata()
        {
            var currentUserId = AbpSession.GetUserId();
            var datasetId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "details-dataset",
                    Description = "Details dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "details.json",
                    CreationTime = new DateTime(2026, 3, 30, 8, 30, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                var rawVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Superseded,
                    ColumnCount = 2,
                    SizeBytes = 128,
                    CreationTime = new DateTime(2026, 3, 30, 8, 35, 0, DateTimeKind.Utc)
                }).Entity;

                var processedVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ParentVersionId = rawVersion.Id,
                    ColumnCount = 3,
                    SizeBytes = 256,
                    SchemaJson = "{\"columns\":3}",
                    CreationTime = new DateTime(2026, 3, 30, 8, 40, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                dataset.CurrentVersionId = processedVersion.Id;

                context.DatasetColumns.AddRange(new[]
                {
                    new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = processedVersion.Id,
                        Name = "customer_id",
                        DataType = "integer",
                        IsDataTypeInferred = true,
                        Ordinal = 1
                    },
                    new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = processedVersion.Id,
                        Name = "name",
                        DataType = "string",
                        IsDataTypeInferred = true,
                        Ordinal = 2
                    },
                    new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = processedVersion.Id,
                        Name = "is_active",
                        DataType = "boolean",
                        IsDataTypeInferred = true,
                        Ordinal = 3
                    }
                });

                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = 1,
                    DatasetVersionId = processedVersion.Id,
                    StorageProvider = "local-filesystem",
                    StorageKey = "tenants/1/datasets/details/versions/2/raw/details.json",
                    OriginalFileName = "details.json",
                    ContentType = "application/json",
                    SizeBytes = 256,
                    ChecksumSha256 = new string('a', 64),
                    CreationTime = new DateTime(2026, 3, 30, 8, 41, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return dataset.Id;
            });

            var output = await _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
            {
                DatasetId = datasetId
            });

            output.Dataset.Id.ShouldBe(datasetId);
            output.Dataset.Name.ShouldBe("details-dataset");
            output.Versions.Count.ShouldBe(2);
            output.Versions[0].VersionNumber.ShouldBe(2);
            output.Versions[1].VersionNumber.ShouldBe(1);
            output.SelectedVersion.ShouldNotBeNull();
            output.SelectedVersion.VersionNumber.ShouldBe(2);
            output.SelectedVersion.ColumnCount.ShouldBe(3);
            output.SelectedVersion.SizeBytes.ShouldBe(256);
            output.SelectedVersion.RawFile.ShouldNotBeNull();
            output.SelectedVersion.RawFile.OriginalFileName.ShouldBe("details.json");
            output.SelectedVersion.RawFile.ContentType.ShouldBe("application/json");
            output.SelectedVersion.RawFile.SizeBytes.ShouldBe(256);
            output.Columns.Select(item => item.Name).ShouldBe(new[] { "customer_id", "name", "is_active" });
        }

        [Fact]
        public async Task GetDetailsAsync_Should_Return_Requested_Selected_Version_When_Provided()
        {
            var currentUserId = AbpSession.GetUserId();
            var seeded = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "selected-version-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "selected.csv"
                }).Entity;

                context.SaveChanges();

                var rawVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Superseded,
                    ColumnCount = 2,
                    SizeBytes = 100
                }).Entity;

                var processedVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ColumnCount = 1,
                    SizeBytes = 120
                }).Entity;

                context.SaveChanges();

                dataset.CurrentVersionId = processedVersion.Id;

                context.DatasetColumns.AddRange(new[]
                {
                    new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = rawVersion.Id,
                        Name = "raw_id",
                        DataType = "string",
                        IsDataTypeInferred = true,
                        Ordinal = 1
                    },
                    new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = processedVersion.Id,
                        Name = "processed_id",
                        DataType = "integer",
                        IsDataTypeInferred = true,
                        Ordinal = 1
                    }
                });

                context.SaveChanges();
                return new DatasetDetailsSeedResult(dataset.Id, rawVersion.Id);
            });

            var output = await _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
            {
                DatasetId = seeded.DatasetId,
                SelectedVersionId = seeded.SelectedVersionId
            });

            output.SelectedVersion.ShouldNotBeNull();
            output.SelectedVersion.Id.ShouldBe(seeded.SelectedVersionId);
            output.SelectedVersion.VersionNumber.ShouldBe(1);
            output.Columns.Count.ShouldBe(1);
            output.Columns.Single().Name.ShouldBe("raw_id");
        }

        [Fact]
        public async Task GetDetailsAsync_Should_Reject_Selected_Version_From_Another_Dataset()
        {
            var currentUserId = AbpSession.GetUserId();
            var seeded = UsingDbContext(context =>
            {
                var firstDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "first-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "first.csv"
                }).Entity;

                var secondDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "second-dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "second.json"
                }).Entity;

                context.SaveChanges();

                var secondDatasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = secondDataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 64
                }).Entity;

                context.SaveChanges();
                return new DatasetDetailsSeedResult(firstDataset.Id, secondDatasetVersion.Id);
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
                {
                    DatasetId = seeded.DatasetId,
                    SelectedVersionId = seeded.SelectedVersionId
                }));

            exception.Message.ShouldBe("The selected dataset version does not belong to the requested dataset.");
        }

        [Fact]
        public async Task GetDetailsAsync_Should_Hide_Dataset_From_Other_Owner_In_Same_Tenant()
        {
            var otherOwnerDatasetId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "other-owner-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId() + 500,
                    OriginalFileName = "other-owner.csv"
                }).Entity;

                context.SaveChanges();
                return dataset.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
                {
                    DatasetId = otherOwnerDatasetId
                }));
        }

        [Fact]
        public async Task GetDetailsAsync_Should_Return_Empty_Selected_Version_When_No_Current_Version_Exists()
        {
            var datasetId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "no-current-version-dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "noversion.json"
                }).Entity;

                context.SaveChanges();
                return dataset.Id;
            });

            var output = await _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
            {
                DatasetId = datasetId
            });

            output.Dataset.Id.ShouldBe(datasetId);
            output.Versions.ShouldBeEmpty();
            output.SelectedVersion.ShouldBeNull();
            output.Columns.ShouldBeEmpty();
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
        public async Task GetAllAsync_Should_Return_Only_Current_User_Datasets_And_Order_Newest_First()
        {
            var currentUserId = AbpSession.GetUserId();
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("datasets-list-other", "Datasets List Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            UsingDbContext(1, context =>
            {
                var olderDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "older-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "older.csv",
                    CreationTime = new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc)
                }).Entity;

                var newerDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "newer-dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "newer.json",
                    CreationTime = new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc)
                }).Entity;

                var hiddenSameTenantDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "same-tenant-other-owner",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = currentUserId + 100,
                    OriginalFileName = "other-owner.csv",
                    CreationTime = new DateTime(2026, 3, 30, 9, 30, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                var olderVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = olderDataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    ColumnCount = 2,
                    SizeBytes = 100
                }).Entity;

                var newerVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = newerDataset.Id,
                    VersionNumber = 3,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ColumnCount = 5,
                    SizeBytes = 200
                }).Entity;

                context.SaveChanges();

                olderDataset.CurrentVersionId = olderVersion.Id;
                newerDataset.CurrentVersionId = newerVersion.Id;
                hiddenSameTenantDataset.CurrentVersionId = null;
                context.SaveChanges();
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
            output.Items[0].CurrentVersionNumber.ShouldBe(3);
            output.Items[0].CurrentVersionStatus.ShouldBe(DatasetVersionStatus.Active);
            output.Items[0].ColumnCount.ShouldBe(5);
            output.Items[1].Name.ShouldBe("older-dataset");
            output.Items[1].CurrentVersionNumber.ShouldBe(1);
            output.Items[1].ColumnCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Keyword_And_Status()
        {
            var currentUserId = AbpSession.GetUserId();
            UsingDbContext(1, context =>
            {
                var salesDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "sales-raw",
                    Description = "Quarterly sales extract",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "sales-raw.csv"
                }).Entity;

                var financeDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "finance-ready",
                    Description = "Finance dataset",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "finance-ready.json"
                }).Entity;

                var noVersionDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "draft-no-version",
                    Description = "No current version yet",
                    SourceFormat = DatasetFormat.Json,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = currentUserId,
                    OriginalFileName = "draft.json"
                }).Entity;

                var hiddenDataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "hidden-other-owner",
                    Description = "Other owner dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = currentUserId + 42,
                    OriginalFileName = "hidden.csv"
                }).Entity;

                context.SaveChanges();

                var financeVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = financeDataset.Id,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ColumnCount = 7,
                    SizeBytes = 120
                }).Entity;

                context.SaveChanges();

                financeDataset.CurrentVersionId = financeVersion.Id;
                context.SaveChanges();
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
            statusOutput.Items.Single().CurrentVersionNumber.ShouldBe(2);
            statusOutput.Items.Single().ColumnCount.ShouldBe(7);

            var noVersionOutput = await _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Keyword = "draft-no-version"
            });

            noVersionOutput.TotalCount.ShouldBe(1);
            noVersionOutput.Items.Single().CurrentVersionId.ShouldBeNull();
            noVersionOutput.Items.Single().CurrentVersionNumber.ShouldBeNull();
            noVersionOutput.Items.Single().CurrentVersionStatus.ShouldBeNull();
            noVersionOutput.Items.Single().ColumnCount.ShouldBeNull();
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

            var detailsException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.GetDetailsAsync(new GetDatasetDetailsInput
                {
                    DatasetId = 1
                }));

            detailsException.Message.ShouldBe("Tenant context is required for dataset operations.");

            var listException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAppService.GetAllAsync(new PagedDatasetResultRequestDto
                {
                    MaxResultCount = 20,
                    SkipCount = 0
                }));

            listException.Message.ShouldBe("Tenant context is required for dataset operations.");
        }

        private class DatasetDetailsSeedResult
        {
            public DatasetDetailsSeedResult(long datasetId, long selectedVersionId)
            {
                DatasetId = datasetId;
                SelectedVersionId = selectedVersionId;
            }

            public long DatasetId { get; }

            public long SelectedVersionId { get; }
        }
    }
}
