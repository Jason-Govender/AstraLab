using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;

namespace AstraLab.Tests.Domains.Datasets
{
    public class DatasetEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Dataset_With_Expected_Values()
        {
            var ownerUserId = AbpSession.GetUserId();
            long datasetId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "sales-orders-2026",
                    Description = "Primary sales order extract",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = ownerUserId,
                    OriginalFileName = "sales-orders-2026.csv"
                };

                context.Datasets.Add(dataset);
                await context.SaveChangesAsync();

                datasetId = dataset.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var dataset = await context.Datasets.SingleAsync(item => item.Id == datasetId);

                dataset.TenantId.ShouldBe(AbpSession.GetTenantId());
                dataset.OwnerUserId.ShouldBe(ownerUserId);
                dataset.Name.ShouldBe("sales-orders-2026");
                dataset.Description.ShouldBe("Primary sales order extract");
                dataset.SourceFormat.ShouldBe(DatasetFormat.Csv);
                dataset.Status.ShouldBe(DatasetStatus.Uploaded);
                dataset.OriginalFileName.ShouldBe("sales-orders-2026.csv");
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Dataset_Queries()
        {
            long datasetId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("secondtenant", "Second Tenant")
                {
                    IsActive = true
                };

                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();

                secondTenantId = tenant.Id;
            });

            await UsingDbContextAsync(1, async context =>
            {
                var dataset = new Dataset
                {
                    TenantId = 1,
                    Name = "tenant-1-dataset",
                    SourceFormat = DatasetFormat.Json,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-1-dataset.json"
                };

                context.Datasets.Add(dataset);
                await context.SaveChangesAsync();

                datasetId = dataset.Id;
            });

            var tenantDatasetCount = await UsingDbContextAsync((int?)null, async context =>
                await context.Datasets.CountAsync(item => item.Id == datasetId && item.TenantId == 1));

            var secondTenantDatasetCount = await UsingDbContextAsync((int?)null, async context =>
                await context.Datasets.CountAsync(item => item.Id == datasetId && item.TenantId == secondTenantId));

            tenantDatasetCount.ShouldBe(1);
            secondTenantDatasetCount.ShouldBe(0);
        }
    }
}
