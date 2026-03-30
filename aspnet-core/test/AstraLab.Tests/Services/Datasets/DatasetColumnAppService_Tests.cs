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
    public class DatasetColumnAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetColumnAppService _datasetColumnAppService;

        public DatasetColumnAppService_Tests()
        {
            _datasetColumnAppService = Resolve<IDatasetColumnAppService>();
        }

        [Fact]
        public async Task ReplaceForVersionAsync_Should_Insert_Ordered_Column_Set_For_Current_Tenant()
        {
            var datasetVersionId = await CreateDatasetVersionAsync();

            var output = await _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
            {
                DatasetVersionId = datasetVersionId,
                Columns = new List<ReplaceDatasetColumnItemDto>
                {
                    new ReplaceDatasetColumnItemDto
                    {
                        Name = "customer_id",
                        DataType = "integer",
                        IsDataTypeInferred = true,
                        Ordinal = 1,
                        NullCount = 0,
                        DistinctCount = 100
                    },
                    new ReplaceDatasetColumnItemDto
                    {
                        Name = "customer_name",
                        DataType = "string",
                        IsDataTypeInferred = false,
                        Ordinal = 2,
                        NullCount = 5,
                        DistinctCount = 95
                    }
                }
            });

            output.Items.Count.ShouldBe(2);
            output.Items[0].Name.ShouldBe("customer_id");
            output.Items[1].Name.ShouldBe("customer_name");

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == datasetVersionId);
                var columns = context.DatasetColumns
                    .Where(item => item.DatasetVersionId == datasetVersionId)
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                datasetVersion.ColumnCount.ShouldBe(2);
                columns.Count.ShouldBe(2);
                columns[0].Ordinal.ShouldBe(1);
                columns[1].Ordinal.ShouldBe(2);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ReplaceForVersionAsync_Should_Remove_Previous_Columns_And_Update_Column_Count()
        {
            var datasetVersionId = await CreateDatasetVersionAsync();

            await SeedColumnsAsync(datasetVersionId,
                new DatasetColumn
                {
                    Name = "legacy_id",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                },
                new DatasetColumn
                {
                    Name = "legacy_name",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 2
                });

            var output = await _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
            {
                DatasetVersionId = datasetVersionId,
                Columns = new List<ReplaceDatasetColumnItemDto>
                {
                    new ReplaceDatasetColumnItemDto
                    {
                        Name = "order_id",
                        DataType = "integer",
                        IsDataTypeInferred = true,
                        Ordinal = 1
                    }
                }
            });

            output.Items.Count.ShouldBe(1);
            output.Items.Single().Name.ShouldBe("order_id");

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == datasetVersionId);
                var columns = context.DatasetColumns.Where(item => item.DatasetVersionId == datasetVersionId).ToList();

                datasetVersion.ColumnCount.ShouldBe(1);
                columns.Count.ShouldBe(1);
                columns.Single().Name.ShouldBe("order_id");
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ReplaceForVersionAsync_Should_Clear_Existing_Columns_When_Empty_List_Is_Submitted()
        {
            var datasetVersionId = await CreateDatasetVersionAsync();

            await SeedColumnsAsync(datasetVersionId,
                new DatasetColumn
                {
                    Name = "to_remove",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

            var output = await _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
            {
                DatasetVersionId = datasetVersionId,
                Columns = new List<ReplaceDatasetColumnItemDto>()
            });

            output.Items.Count.ShouldBe(0);

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == datasetVersionId);
                var columnCount = context.DatasetColumns.Count(item => item.DatasetVersionId == datasetVersionId);

                datasetVersion.ColumnCount.ShouldBe(0);
                columnCount.ShouldBe(0);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ReplaceForVersionAsync_Should_Reject_Duplicate_Ordinals()
        {
            var datasetVersionId = await CreateDatasetVersionAsync();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
                {
                    DatasetVersionId = datasetVersionId,
                    Columns = new List<ReplaceDatasetColumnItemDto>
                    {
                        new ReplaceDatasetColumnItemDto
                        {
                            Name = "first",
                            DataType = "integer",
                            IsDataTypeInferred = true,
                            Ordinal = 1
                        },
                        new ReplaceDatasetColumnItemDto
                        {
                            Name = "second",
                            DataType = "string",
                            IsDataTypeInferred = true,
                            Ordinal = 1
                        }
                    }
                }));

            exception.Message.ShouldBe("Column ordinals must be unique within the dataset version.");
        }

        [Fact]
        public async Task ReplaceForVersionAsync_Should_Reject_Dataset_Version_From_Other_Tenant()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("dataset-column-other", "Dataset Column Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantDatasetVersionId = UsingDbContext(otherTenantId, context =>
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

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = otherTenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 400
                }).Entity;

                context.SaveChanges();
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
                {
                    DatasetVersionId = otherTenantDatasetVersionId,
                    Columns = new List<ReplaceDatasetColumnItemDto>()
                }));
        }

        [Fact]
        public async Task GetAsync_Should_Hide_Column_From_Other_Tenant()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("dataset-column-get-other", "Dataset Column Get Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantColumnId = UsingDbContext(otherTenantId, context =>
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

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = otherTenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 500
                }).Entity;

                context.SaveChanges();

                var datasetColumn = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = otherTenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "other_tenant_column",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                context.SaveChanges();
                return datasetColumn.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetColumnAppService.GetAsync(new EntityDto<long>(otherTenantColumnId)));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Columns_For_Requested_Dataset_Version_In_Ordinal_Order()
        {
            var datasetVersionId = await CreateDatasetVersionAsync();
            var otherDatasetVersionId = await CreateDatasetVersionAsync("other-columns-dataset", "other-columns.csv");

            await SeedColumnsAsync(datasetVersionId,
                new DatasetColumn
                {
                    Name = "second_column",
                    DataType = "string",
                    IsDataTypeInferred = false,
                    Ordinal = 2
                },
                new DatasetColumn
                {
                    Name = "first_column",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

            await SeedColumnsAsync(otherDatasetVersionId,
                new DatasetColumn
                {
                    Name = "other_column",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

            var output = await _datasetColumnAppService.GetAllAsync(new PagedDatasetColumnResultRequestDto
            {
                DatasetVersionId = datasetVersionId,
                MaxResultCount = 20,
                SkipCount = 0
            });

            output.TotalCount.ShouldBe(2);
            output.Items.Count.ShouldBe(2);
            output.Items[0].Ordinal.ShouldBe(1);
            output.Items[0].Name.ShouldBe("first_column");
            output.Items[1].Ordinal.ShouldBe(2);
            output.Items[1].Name.ShouldBe("second_column");
        }

        [Fact]
        public async Task Host_Context_Should_Be_Rejected_For_Dataset_Column_Operations()
        {
            LoginAsHostAdmin();

            var replaceException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetColumnAppService.ReplaceForVersionAsync(new ReplaceDatasetColumnsDto
                {
                    DatasetVersionId = 1,
                    Columns = new List<ReplaceDatasetColumnItemDto>()
                }));

            replaceException.Message.ShouldBe("Tenant context is required for dataset column operations.");

            var getException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetColumnAppService.GetAsync(new EntityDto<long>(1)));

            getException.Message.ShouldBe("Tenant context is required for dataset column operations.");

            var listException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetColumnAppService.GetAllAsync(new PagedDatasetColumnResultRequestDto
                {
                    DatasetVersionId = 1,
                    MaxResultCount = 20,
                    SkipCount = 0
                }));

            listException.Message.ShouldBe("Tenant context is required for dataset column operations.");
        }

        private async Task<long> CreateDatasetVersionAsync(string datasetName = "dataset-columns", string originalFileName = "dataset-columns.csv")
        {
            return await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = datasetName,
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = originalFileName
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 256
                }).Entity;

                await context.SaveChangesAsync();
                return datasetVersion.Id;
            });
        }

        private async Task SeedColumnsAsync(long datasetVersionId, params DatasetColumn[] datasetColumns)
        {
            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == datasetVersionId);

                foreach (var datasetColumn in datasetColumns)
                {
                    datasetColumn.TenantId = datasetVersion.TenantId;
                    datasetColumn.DatasetVersionId = datasetVersionId;
                    context.DatasetColumns.Add(datasetColumn);
                }

                datasetVersion.ColumnCount = datasetColumns.Length;
                await context.SaveChangesAsync();
            });
        }
    }
}
