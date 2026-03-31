using System;
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
    public class DatasetTransformationEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Dataset_Transformation_With_Source_And_Result_Versions_And_Load_The_Full_Relationship_Graph()
        {
            long sourceDatasetVersionId = 0;
            long resultDatasetVersionId = 0;
            long transformationId = 0;
            DateTime executedAt = new DateTime(2026, 3, 31, 9, 15, 0, DateTimeKind.Utc);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "transformable-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "transformable.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var sourceVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 1024
                }).Entity;

                await context.SaveChangesAsync();

                var resultVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 2,
                    VersionType = DatasetVersionType.Processed,
                    Status = DatasetVersionStatus.Active,
                    ParentVersionId = sourceVersion.Id,
                    SizeBytes = 768
                }).Entity;

                await context.SaveChangesAsync();

                var transformation = context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = dataset.TenantId,
                    SourceDatasetVersionId = sourceVersion.Id,
                    ResultDatasetVersionId = resultVersion.Id,
                    TransformationType = DatasetTransformationType.RemoveDuplicates,
                    ConfigurationJson = "{\"strategy\":\"exact-match\"}",
                    ExecutionOrder = 1,
                    ExecutedAt = executedAt,
                    SummaryJson = "{\"removedRows\":12}"
                }).Entity;

                await context.SaveChangesAsync();

                sourceDatasetVersionId = sourceVersion.Id;
                resultDatasetVersionId = resultVersion.Id;
                transformationId = transformation.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var sourceVersion = await context.DatasetVersions
                    .Include(item => item.OutgoingTransformations)
                    .SingleAsync(item => item.Id == sourceDatasetVersionId);

                sourceVersion.OutgoingTransformations.Count.ShouldBe(1);
                sourceVersion.OutgoingTransformations.Single().Id.ShouldBe(transformationId);

                var transformation = await context.DatasetTransformations
                    .Include(item => item.SourceDatasetVersion)
                    .Include(item => item.ResultDatasetVersion)
                    .SingleAsync(item => item.Id == transformationId);

                transformation.SourceDatasetVersion.Id.ShouldBe(sourceDatasetVersionId);
                transformation.ResultDatasetVersion.Id.ShouldBe(resultDatasetVersionId);
                transformation.TransformationType.ShouldBe(DatasetTransformationType.RemoveDuplicates);
                transformation.ConfigurationJson.ShouldBe("{\"strategy\":\"exact-match\"}");
                transformation.ExecutionOrder.ShouldBe(1);
                transformation.ExecutedAt.ShouldBe(executedAt);
                transformation.SummaryJson.ShouldBe("{\"removedRows\":12}");

                var resultVersion = await context.DatasetVersions
                    .Include(item => item.ProducedByTransformation)
                    .SingleAsync(item => item.Id == resultDatasetVersionId);

                resultVersion.ProducedByTransformation.ShouldNotBeNull();
                resultVersion.ProducedByTransformation.Id.ShouldBe(transformationId);
            });
        }

        [Fact]
        public async Task Should_Persist_Dataset_Transformation_With_Only_A_Source_Version()
        {
            long transformationId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "source-only-transformation-dataset",
                    SourceFormat = DatasetFormat.Json,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "source-only.json"
                }).Entity;

                await context.SaveChangesAsync();

                var sourceVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 333
                }).Entity;

                await context.SaveChangesAsync();

                var transformation = context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = dataset.TenantId,
                    SourceDatasetVersionId = sourceVersion.Id,
                    TransformationType = DatasetTransformationType.FilterRows,
                    ConfigurationJson = "{\"predicate\":\"status == 'active'\"}",
                    ExecutionOrder = 1,
                    ExecutedAt = DateTime.UtcNow
                }).Entity;

                await context.SaveChangesAsync();
                transformationId = transformation.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var transformation = await context.DatasetTransformations.SingleAsync(item => item.Id == transformationId);

                transformation.ResultDatasetVersionId.ShouldBeNull();
                transformation.SummaryJson.ShouldBeNull();
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Transformation_Queries()
        {
            long transformationId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("datasettransformationtenant", "Dataset Transformation Tenant")
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
                    Name = "tenant-transformation-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-transform.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var sourceVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 120
                }).Entity;

                await context.SaveChangesAsync();

                var transformation = context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = 1,
                    SourceDatasetVersionId = sourceVersion.Id,
                    TransformationType = DatasetTransformationType.Aggregate,
                    ConfigurationJson = "{\"groupBy\":[\"region\"]}",
                    ExecutionOrder = 1,
                    ExecutedAt = DateTime.UtcNow
                }).Entity;

                await context.SaveChangesAsync();
                transformationId = transformation.Id;
            });

            var tenantTransformationCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetTransformations.CountAsync(item => item.Id == transformationId && item.TenantId == 1));

            var secondTenantTransformationCount = await UsingDbContextAsync((int?)null, async context =>
                await context.DatasetTransformations.CountAsync(item => item.Id == transformationId && item.TenantId == secondTenantId));

            tenantTransformationCount.ShouldBe(1);
            secondTenantTransformationCount.ShouldBe(0);
        }

        [Fact]
        public void Should_Define_Unique_Execution_Order_Index_Per_Source_Dataset_Version_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetTransformation));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetTransformation.SourceDatasetVersionId), nameof(DatasetTransformation.ExecutionOrder) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public void Should_Define_Unique_Result_Dataset_Version_Index_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetTransformation));
                var uniqueIndex = entityType.GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(DatasetTransformation.ResultDatasetVersionId) }));

                uniqueIndex.IsUnique.ShouldBeTrue();
            });
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Source_Dataset_Version_To_Dataset_Transformation_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetTransformation));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetTransformation.SourceDatasetVersionId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetVersion));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Restrict_Delete_From_Result_Dataset_Version_To_Dataset_Transformation_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetTransformation));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(key =>
                        key.Properties.Single().Name == nameof(DatasetTransformation.ResultDatasetVersionId) &&
                        key.PrincipalEntityType.ClrType == typeof(DatasetVersion));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
            });
        }
    }
}
