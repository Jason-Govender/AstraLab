using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.AI;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class AiDatasetContextBuilder_Tests : AstraLabTestBase
    {
        private readonly IAiDatasetContextBuilder _aiDatasetContextBuilder;

        public AiDatasetContextBuilder_Tests()
        {
            _aiDatasetContextBuilder = Resolve<IAiDatasetContextBuilder>();
        }

        [Fact]
        public async Task BuildAsync_Should_Return_Structured_Context_For_A_Profiled_Dataset_Version()
        {
            var currentUserId = AbpSession.GetUserId();
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-context-profiled", "Context description", currentUserId);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                datasetVersion.SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}";
                datasetVersion.RowCount = 3;
                datasetVersion.ColumnCount = 2;

                var amountColumn = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "amount",
                    DataType = "decimal",
                    IsDataTypeInferred = true,
                    Ordinal = 1,
                    NullCount = 1,
                    DistinctCount = 2
                }).Entity;

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "customer_id",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 2,
                    NullCount = 0,
                    DistinctCount = 3
                });

                var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    RowCount = 3,
                    DuplicateRowCount = 1,
                    DataHealthScore = 88.50m,
                    SummaryJson = "{\"totalNullCount\":1,\"overallNullPercentage\":16.67,\"totalAnomalyCount\":1,\"overallAnomalyPercentage\":33.33}",
                    CreationTime = new DateTime(2026, 4, 2, 10, 15, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.DatasetColumnProfiles.Add(new DatasetColumnProfile
                {
                    TenantId = 1,
                    DatasetProfileId = datasetProfile.Id,
                    DatasetColumnId = amountColumn.Id,
                    InferredDataType = "decimal",
                    NullCount = 1,
                    DistinctCount = 2,
                    StatisticsJson = "{\"nullPercentage\":33.33,\"mean\":10.5,\"min\":1.0,\"max\":20.0,\"anomalyCount\":1,\"anomalyPercentage\":33.33,\"hasAnomalies\":true}"
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            var output = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, AbpSession.GetTenantId(), currentUserId);

            output.Dataset.DatasetId.ShouldBeGreaterThan(0L);
            output.Dataset.Name.ShouldBe("ai-context-profiled");
            output.Version.DatasetVersionId.ShouldBe(datasetVersionId);
            output.Schema.HasSchemaJson.ShouldBeTrue();
            output.Schema.TotalColumnCount.ShouldBe(2);
            output.Profiling.ShouldNotBeNull();
            output.Profiling.DataHealthScore.ShouldBe(88.50m);
            output.Columns.Count.ShouldBe(2);

            var amountColumn = output.Columns.Single(item => item.Name == "amount");
            amountColumn.HasDetailedProfile.ShouldBeTrue();
            amountColumn.ProfiledInferredDataType.ShouldBe("decimal");
            amountColumn.NullPercentage.ShouldBe(33.33m);
            amountColumn.HasAnomalies.ShouldBe(true);

            var customerIdColumn = output.Columns.Single(item => item.Name == "customer_id");
            customerIdColumn.HasDetailedProfile.ShouldBeFalse();
            customerIdColumn.NullCount.ShouldBe(0);
            customerIdColumn.DistinctCount.ShouldBe(3);
        }

        [Fact]
        public async Task BuildAsync_Should_Return_Valid_Context_When_Profile_Is_Missing()
        {
            var currentUserId = AbpSession.GetUserId();
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-context-unprofiled", null, currentUserId);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "name",
                    DataType = "string",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            var output = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, AbpSession.GetTenantId(), currentUserId);

            output.Dataset.Name.ShouldBe("ai-context-unprofiled");
            output.Profiling.ShouldBeNull();
            output.Columns.Count.ShouldBe(1);
            output.Columns.Single().HasDetailedProfile.ShouldBeFalse();
        }

        [Fact]
        public async Task BuildAsync_Should_Prune_Detailed_Column_Context_For_Wide_Datasets_And_Keep_High_Risk_Columns()
        {
            var currentUserId = AbpSession.GetUserId();
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-context-wide", "wide dataset", currentUserId);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    RowCount = 100,
                    DuplicateRowCount = 0,
                    DataHealthScore = 70m,
                    SummaryJson = "{\"totalNullCount\":100,\"overallNullPercentage\":10.0,\"totalAnomalyCount\":10,\"overallAnomalyPercentage\":5.0}"
                }).Entity;

                context.SaveChanges();

                for (var ordinal = 1; ordinal <= 25; ordinal++)
                {
                    var column = context.DatasetColumns.Add(new DatasetColumn
                    {
                        TenantId = 1,
                        DatasetVersionId = datasetVersion.Id,
                        Name = ordinal == 1 ? "high_nulls" : ordinal == 2 ? "anomaly_amount" : "column_" + ordinal,
                        DataType = ordinal % 2 == 0 ? "decimal" : "string",
                        IsDataTypeInferred = true,
                        Ordinal = ordinal,
                        NullCount = ordinal == 1 ? 60 : 0,
                        DistinctCount = ordinal == 2 ? 90 : ordinal
                    }).Entity;

                    context.SaveChanges();

                    var statisticsJson = ordinal == 1
                        ? "{\"nullPercentage\":60.0,\"mean\":null,\"min\":null,\"max\":null,\"anomalyCount\":0,\"anomalyPercentage\":0.0,\"hasAnomalies\":false}"
                        : ordinal == 2
                            ? "{\"nullPercentage\":0.0,\"mean\":25.0,\"min\":1.0,\"max\":999.0,\"anomalyCount\":5,\"anomalyPercentage\":25.0,\"hasAnomalies\":true}"
                            : "{\"nullPercentage\":0.0,\"mean\":1.0,\"min\":1.0,\"max\":1.0,\"anomalyCount\":0,\"anomalyPercentage\":0.0,\"hasAnomalies\":false}";

                    context.DatasetColumnProfiles.Add(new DatasetColumnProfile
                    {
                        TenantId = 1,
                        DatasetProfileId = datasetProfile.Id,
                        DatasetColumnId = column.Id,
                        InferredDataType = column.DataType,
                        NullCount = column.NullCount ?? 0,
                        DistinctCount = column.DistinctCount,
                        StatisticsJson = statisticsJson
                    });
                }

                context.SaveChanges();
                return datasetVersion.Id;
            });

            var output = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, AbpSession.GetTenantId(), currentUserId);

            output.Columns.Count.ShouldBe(25);
            output.IsColumnContextPruned.ShouldBeTrue();
            output.DetailedColumnCount.ShouldBe(AiDatasetContextDefaults.MaxProfiledColumnsInCompactSummary);

            var highNullColumn = output.Columns.Single(item => item.Name == "high_nulls");
            var anomalyColumn = output.Columns.Single(item => item.Name == "anomaly_amount");

            highNullColumn.HasDetailedProfile.ShouldBeTrue();
            anomalyColumn.HasDetailedProfile.ShouldBeTrue();
            anomalyColumn.HasAnomalies.ShouldBe(true);
        }

        [Fact]
        public async Task BuildAsync_Should_Throw_When_Dataset_Version_Belongs_To_A_Different_Owner()
        {
            var currentUserId = AbpSession.GetUserId();
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-context-hidden", null, currentUserId + 10);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _aiDatasetContextBuilder.BuildAsync(datasetVersionId, AbpSession.GetTenantId(), currentUserId));
        }

        [Fact]
        public async Task BuildAsync_Should_Throw_When_Dataset_Version_Belongs_To_A_Different_Tenant()
        {
            var currentUserId = AbpSession.GetUserId();
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("ai-context-other-tenant", "AI Context Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var datasetVersionId = UsingDbContext(otherTenantId, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = otherTenantId,
                    Name = "ai-context-other-tenant-dataset",
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
                    SizeBytes = 15
                }).Entity;

                context.SaveChanges();
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _aiDatasetContextBuilder.BuildAsync(datasetVersionId, AbpSession.GetTenantId(), currentUserId));
        }

        private static Dataset CreateDataset(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            string name,
            string description,
            long ownerUserId)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = 1,
                Name = name,
                Description = description,
                SourceFormat = DatasetFormat.Csv,
                Status = DatasetStatus.Ready,
                OwnerUserId = ownerUserId,
                OriginalFileName = name + ".csv"
            }).Entity;

            context.SaveChanges();
            return dataset;
        }

        private static DatasetVersion CreateDatasetVersion(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetId,
            int versionNumber,
            DatasetVersionType versionType)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = 1,
                DatasetId = datasetId,
                VersionNumber = versionNumber,
                VersionType = versionType,
                Status = DatasetVersionStatus.Active,
                SizeBytes = 128,
                CreationTime = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
            }).Entity;

            context.SaveChanges();
            return datasetVersion;
        }
    }
}
