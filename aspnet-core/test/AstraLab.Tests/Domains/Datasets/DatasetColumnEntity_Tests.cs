using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Domains.Datasets
{
    public class DatasetColumnEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Dataset_Version_With_Columns_And_Read_Them_In_Ordinal_Order()
        {
            long datasetVersionId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "columns-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "columns.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    ColumnCount = 2,
                    SizeBytes = 1024
                }).Entity;

                await context.SaveChangesAsync();

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "customer_id",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1,
                    NullCount = 0,
                    DistinctCount = 500
                });

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "customer_name",
                    DataType = "string",
                    IsDataTypeInferred = false,
                    Ordinal = 2,
                    NullCount = 12,
                    DistinctCount = 480
                });

                await context.SaveChangesAsync();
                datasetVersionId = datasetVersion.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = await context.DatasetVersions
                    .Include(item => item.Columns)
                    .SingleAsync(item => item.Id == datasetVersionId);

                datasetVersion.Columns.Count.ShouldBe(2);

                var orderedColumns = datasetVersion.Columns
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                orderedColumns[0].Name.ShouldBe("customer_id");
                orderedColumns[0].DataType.ShouldBe("integer");
                orderedColumns[0].IsDataTypeInferred.ShouldBeTrue();
                orderedColumns[0].Ordinal.ShouldBe(1);
                orderedColumns[0].NullCount.ShouldBe(0);
                orderedColumns[0].DistinctCount.ShouldBe(500);

                orderedColumns[1].Name.ShouldBe("customer_name");
                orderedColumns[1].DataType.ShouldBe("string");
                orderedColumns[1].IsDataTypeInferred.ShouldBeFalse();
                orderedColumns[1].Ordinal.ShouldBe(2);
                orderedColumns[1].NullCount.ShouldBe(12);
                orderedColumns[1].DistinctCount.ShouldBe(480);
            });
        }

        [Fact]
        public void Should_Define_Unique_Ordinal_Index_Per_Dataset_Version_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetColumn));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetColumn.DatasetVersionId), nameof(DatasetColumn.Ordinal) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Dataset_Column_Queries()
        {
            long datasetColumnId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("datasetcolumntenant", "Dataset Column Tenant")
                {
                    IsActive = true
                };

                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();

                secondTenantId = tenant.Id;
            });

            await UsingDbContextAsync(1, async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "tenant-column-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-column.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                await context.SaveChangesAsync();

                var datasetColumn = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "tenant_column",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                await context.SaveChangesAsync();
                datasetColumnId = datasetColumn.Id;
            });

            var tenantColumnCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetColumns.CountAsync(item => item.Id == datasetColumnId && item.TenantId == 1));

            var secondTenantColumnCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetColumns.CountAsync(item => item.Id == datasetColumnId && item.TenantId == secondTenantId));

            tenantColumnCount.ShouldBe(1);
            secondTenantColumnCount.ShouldBe(0);
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Dataset_Version_To_Dataset_Column_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetColumn));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetColumn.DatasetVersionId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetVersion));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }
    }
}
