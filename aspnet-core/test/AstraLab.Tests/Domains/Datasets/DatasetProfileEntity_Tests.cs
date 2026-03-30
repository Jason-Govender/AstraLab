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
    public class DatasetProfileEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Dataset_Profile_With_Column_Profiles_And_Load_The_Full_Relationship_Graph()
        {
            long datasetVersionId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "profiled-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "profiled.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 2048
                }).Entity;

                await context.SaveChangesAsync();

                var firstColumn = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "customer_id",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                var secondColumn = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "customer_name",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 2
                }).Entity;

                await context.SaveChangesAsync();

                var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    RowCount = 500,
                    DuplicateRowCount = 5,
                    DataHealthScore = 96.25m,
                    SummaryJson = "{\"quality\":\"good\"}"
                }).Entity;

                await context.SaveChangesAsync();

                context.DatasetColumnProfiles.Add(new DatasetColumnProfile
                {
                    TenantId = dataset.TenantId,
                    DatasetProfileId = datasetProfile.Id,
                    DatasetColumnId = firstColumn.Id,
                    InferredDataType = "integer",
                    NullCount = 0,
                    DistinctCount = 500,
                    StatisticsJson = "{\"min\":1,\"max\":500}"
                });

                context.DatasetColumnProfiles.Add(new DatasetColumnProfile
                {
                    TenantId = dataset.TenantId,
                    DatasetProfileId = datasetProfile.Id,
                    DatasetColumnId = secondColumn.Id,
                    InferredDataType = "string",
                    NullCount = 8,
                    DistinctCount = 492,
                    StatisticsJson = "{\"topValues\":[\"Alice\"]}"
                });

                await context.SaveChangesAsync();
                datasetVersionId = datasetVersion.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = await context.DatasetVersions
                    .Include(item => item.Profile)
                        .ThenInclude(item => item.ColumnProfiles)
                    .SingleAsync(item => item.Id == datasetVersionId);

                datasetVersion.Profile.ShouldNotBeNull();
                datasetVersion.Profile.RowCount.ShouldBe(500);
                datasetVersion.Profile.DuplicateRowCount.ShouldBe(5);
                datasetVersion.Profile.DataHealthScore.ShouldBe(96.25m);
                datasetVersion.Profile.SummaryJson.ShouldBe("{\"quality\":\"good\"}");
                datasetVersion.Profile.ColumnProfiles.Count.ShouldBe(2);

                var columnProfiles = await context.DatasetColumnProfiles
                    .Include(item => item.DatasetColumn)
                    .Where(item => item.DatasetProfileId == datasetVersion.Profile.Id)
                    .OrderBy(item => item.DatasetColumn.Ordinal)
                    .ToListAsync();

                columnProfiles[0].DatasetColumn.Name.ShouldBe("customer_id");
                columnProfiles[0].InferredDataType.ShouldBe("integer");
                columnProfiles[0].NullCount.ShouldBe(0);
                columnProfiles[0].DistinctCount.ShouldBe(500);
                columnProfiles[0].StatisticsJson.ShouldBe("{\"min\":1,\"max\":500}");

                columnProfiles[1].DatasetColumn.Name.ShouldBe("customer_name");
                columnProfiles[1].InferredDataType.ShouldBe("string");
                columnProfiles[1].NullCount.ShouldBe(8);
                columnProfiles[1].DistinctCount.ShouldBe(492);
                columnProfiles[1].StatisticsJson.ShouldBe("{\"topValues\":[\"Alice\"]}");
            });
        }

        [Fact]
        public void Should_Define_Unique_Profile_Index_Per_Dataset_Version_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetProfile));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetProfile.DatasetVersionId) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public void Should_Define_Unique_Column_Profile_Index_Per_Profile_And_Column_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetColumnProfile));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetColumnProfile.DatasetProfileId), nameof(DatasetColumnProfile.DatasetColumnId) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Profile_Queries()
        {
            long datasetProfileId = 0;
            long datasetColumnProfileId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("datasetprofiletenant", "Dataset Profile Tenant")
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
                    Name = "tenant-profile-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-profile.csv"
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
                    Name = "tenant_profile_column",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                await context.SaveChangesAsync();

                var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    RowCount = 10,
                    DuplicateRowCount = 1,
                    DataHealthScore = 80.00m
                }).Entity;

                await context.SaveChangesAsync();

                var datasetColumnProfile = context.DatasetColumnProfiles.Add(new DatasetColumnProfile
                {
                    TenantId = 1,
                    DatasetProfileId = datasetProfile.Id,
                    DatasetColumnId = datasetColumn.Id,
                    InferredDataType = "integer",
                    NullCount = 0
                }).Entity;

                await context.SaveChangesAsync();

                datasetProfileId = datasetProfile.Id;
                datasetColumnProfileId = datasetColumnProfile.Id;
            });

            var tenantProfileCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetProfiles.CountAsync(item => item.Id == datasetProfileId && item.TenantId == 1));

            var secondTenantProfileCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetProfiles.CountAsync(item => item.Id == datasetProfileId && item.TenantId == secondTenantId));

            var tenantColumnProfileCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetColumnProfiles.CountAsync(item => item.Id == datasetColumnProfileId && item.TenantId == 1));

            var secondTenantColumnProfileCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetColumnProfiles.CountAsync(item => item.Id == datasetColumnProfileId && item.TenantId == secondTenantId));

            tenantProfileCount.ShouldBe(1);
            secondTenantProfileCount.ShouldBe(0);
            tenantColumnProfileCount.ShouldBe(1);
            secondTenantColumnProfileCount.ShouldBe(0);
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Dataset_Version_To_Dataset_Profile_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetProfile));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetProfile.DatasetVersionId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetVersion));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Dataset_Profile_To_Dataset_Column_Profile_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetColumnProfile));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetColumnProfile.DatasetProfileId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetProfile));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Restrict_Delete_From_Dataset_Column_To_Dataset_Column_Profile_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetColumnProfile));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetColumnProfile.DatasetColumnId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetColumn));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
            });
        }
    }
}
