using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;

namespace AstraLab.Tests.Domains.Datasets
{
    public class DatasetVersionEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Dataset_With_Initial_Raw_Version_And_Current_Version()
        {
            long datasetId = 0;
            long versionId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "orders-dataset",
                    Description = "Dataset with initial raw version",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "orders.csv"
                };

                context.Datasets.Add(dataset);
                await context.SaveChangesAsync();

                var datasetVersion = new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    RowCount = 125,
                    ColumnCount = 8,
                    SchemaJson = "{\"columns\":8}",
                    SizeBytes = 2048
                };

                context.DatasetVersions.Add(datasetVersion);
                await context.SaveChangesAsync();

                dataset.CurrentVersionId = datasetVersion.Id;
                await context.SaveChangesAsync();

                datasetId = dataset.Id;
                versionId = datasetVersion.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var dataset = await context.Datasets
                    .Include(item => item.CurrentVersion)
                    .Include(item => item.Versions)
                    .SingleAsync(item => item.Id == datasetId);

                dataset.CurrentVersionId.ShouldBe(versionId);
                dataset.CurrentVersion.ShouldNotBeNull();
                dataset.CurrentVersion.Id.ShouldBe(versionId);
                dataset.Versions.Count.ShouldBe(1);

                var version = dataset.Versions.Single();
                version.VersionNumber.ShouldBe(1);
                version.VersionType.ShouldBe(DatasetVersionType.Raw);
                version.Status.ShouldBe(DatasetVersionStatus.Active);
                version.RowCount.ShouldBe(125);
                version.ColumnCount.ShouldBe(8);
                version.SchemaJson.ShouldBe("{\"columns\":8}");
                version.SizeBytes.ShouldBe(2048);
            });
        }

        [Fact]
        public async Task Should_Persist_Processed_Version_With_Parent_Lineage()
        {
            long rawVersionId = 0;
            long processedVersionId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "lineage-dataset",
                    SourceFormat = DatasetFormat.Json,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "lineage.json"
                }).Entity;

                await context.SaveChangesAsync();

                var rawVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Superseded,
                    SizeBytes = 3000
                }).Entity;

                await context.SaveChangesAsync();

                var processedVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ParentVersionId = rawVersion.Id,
                    RowCount = 75,
                    ColumnCount = 6,
                    SizeBytes = 1500
                }).Entity;

                await context.SaveChangesAsync();

                dataset.CurrentVersionId = processedVersion.Id;
                await context.SaveChangesAsync();

                rawVersionId = rawVersion.Id;
                processedVersionId = processedVersion.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var processedVersion = await context.DatasetVersions
                    .Include(item => item.ParentVersion)
                    .SingleAsync(item => item.Id == processedVersionId);

                processedVersion.ParentVersionId.ShouldBe(rawVersionId);
                processedVersion.ParentVersion.ShouldNotBeNull();
                processedVersion.ParentVersion.Id.ShouldBe(rawVersionId);
                processedVersion.VersionType.ShouldBe(DatasetVersionType.Processed);
                processedVersion.Status.ShouldBe(DatasetVersionStatus.Active);
            });
        }

        [Fact]
        public void Should_Define_Unique_Version_Number_Index_Per_Dataset_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetVersion));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetVersion.DatasetId), nameof(DatasetVersion.VersionNumber) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Dataset_Version_Queries()
        {
            long versionId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("datasetversiontenant", "Dataset Version Tenant")
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
                    Name = "tenant-version-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-version.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var version = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                await context.SaveChangesAsync();
                versionId = version.Id;
            });

            var tenantVersionCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetVersions.CountAsync(item => item.Id == versionId && item.TenantId == 1));

            var secondTenantVersionCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetVersions.CountAsync(item => item.Id == versionId && item.TenantId == secondTenantId));

            tenantVersionCount.ShouldBe(1);
            secondTenantVersionCount.ShouldBe(0);
        }
    }
}
