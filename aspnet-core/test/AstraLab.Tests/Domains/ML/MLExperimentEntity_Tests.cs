using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Domains.ML
{
    public class MLExperimentEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_A_Completed_Experiment_With_Model_Metrics_And_Feature_Importance()
        {
            long datasetVersionId = 0;
            long targetColumnId = 0;
            long firstFeatureColumnId = 0;
            long secondFeatureColumnId = 0;
            long experimentId = 0;
            DateTime executedAt = new DateTime(2026, 4, 1, 8, 30, 0, DateTimeKind.Utc);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "ml-experiment-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "ml.csv"
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

                var featureOne = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "age",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                var featureTwo = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "income",
                    DataType = "decimal",
                    IsDataTypeInferred = true,
                    Ordinal = 2
                }).Entity;

                var target = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "will_buy",
                    DataType = "boolean",
                    IsDataTypeInferred = true,
                    Ordinal = 3
                }).Entity;

                await context.SaveChangesAsync();

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    TargetDatasetColumnId = target.Id,
                    Status = MLExperimentStatus.Completed,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "random_forest_classifier",
                    TrainingConfigurationJson = "{\"algorithm\":\"random_forest\",\"split\":0.8}",
                    ExecutedAt = executedAt
                }).Entity;

                await context.SaveChangesAsync();

                context.MLExperimentFeatures.Add(new MLExperimentFeature
                {
                    TenantId = dataset.TenantId,
                    MLExperimentId = experiment.Id,
                    DatasetColumnId = featureOne.Id,
                    Ordinal = 1
                });

                context.MLExperimentFeatures.Add(new MLExperimentFeature
                {
                    TenantId = dataset.TenantId,
                    MLExperimentId = experiment.Id,
                    DatasetColumnId = featureTwo.Id,
                    Ordinal = 2
                });

                var model = context.MLModels.Add(new MLModel
                {
                    TenantId = dataset.TenantId,
                    MLExperimentId = experiment.Id,
                    ModelType = "random_forest_classifier",
                    ArtifactStorageProvider = "local",
                    ArtifactStorageKey = "ml/models/experiment-1.bin",
                    PerformanceSummaryJson = "{\"bestMetric\":\"accuracy\"}"
                }).Entity;

                await context.SaveChangesAsync();

                context.MLModelMetrics.Add(new MLModelMetric
                {
                    TenantId = dataset.TenantId,
                    MLModelId = model.Id,
                    MetricName = "accuracy",
                    MetricValue = 0.912345m
                });

                context.MLModelMetrics.Add(new MLModelMetric
                {
                    TenantId = dataset.TenantId,
                    MLModelId = model.Id,
                    MetricName = "f1",
                    MetricValue = 0.887654m
                });

                context.MLModelFeatureImportances.Add(new MLModelFeatureImportance
                {
                    TenantId = dataset.TenantId,
                    MLModelId = model.Id,
                    DatasetColumnId = featureOne.Id,
                    ImportanceScore = 0.625000m,
                    Rank = 1
                });

                context.MLModelFeatureImportances.Add(new MLModelFeatureImportance
                {
                    TenantId = dataset.TenantId,
                    MLModelId = model.Id,
                    DatasetColumnId = featureTwo.Id,
                    ImportanceScore = 0.375000m,
                    Rank = 2
                });

                await context.SaveChangesAsync();

                datasetVersionId = datasetVersion.Id;
                targetColumnId = target.Id;
                firstFeatureColumnId = featureOne.Id;
                secondFeatureColumnId = featureTwo.Id;
                experimentId = experiment.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var datasetVersion = await context.DatasetVersions
                    .Include(item => item.MlExperiments)
                    .SingleAsync(item => item.Id == datasetVersionId);

                datasetVersion.MlExperiments.Count.ShouldBe(1);

                var experiment = await context.MLExperiments
                    .Include(item => item.TargetDatasetColumn)
                    .Include(item => item.SelectedFeatures)
                        .ThenInclude(item => item.DatasetColumn)
                    .Include(item => item.Model)
                        .ThenInclude(item => item.Metrics)
                    .Include(item => item.Model)
                        .ThenInclude(item => item.FeatureImportances)
                            .ThenInclude(item => item.DatasetColumn)
                    .SingleAsync(item => item.Id == experimentId);

                experiment.TargetDatasetColumnId.ShouldBe(targetColumnId);
                experiment.TargetDatasetColumn.Name.ShouldBe("will_buy");
                experiment.Status.ShouldBe(MLExperimentStatus.Completed);
                experiment.TaskType.ShouldBe(MLTaskType.Classification);
                experiment.AlgorithmKey.ShouldBe("random_forest_classifier");
                experiment.TrainingConfigurationJson.ShouldBe("{\"algorithm\":\"random_forest\",\"split\":0.8}");
                experiment.ExecutedAt.ShouldBe(executedAt);
                experiment.FailureMessage.ShouldBeNull();
                experiment.SelectedFeatures.OrderBy(item => item.Ordinal).Select(item => item.DatasetColumnId).ShouldBe(new[] { firstFeatureColumnId, secondFeatureColumnId });

                experiment.Model.ShouldNotBeNull();
                experiment.Model.ModelType.ShouldBe("random_forest_classifier");
                experiment.Model.ArtifactStorageProvider.ShouldBe("local");
                experiment.Model.ArtifactStorageKey.ShouldBe("ml/models/experiment-1.bin");
                experiment.Model.PerformanceSummaryJson.ShouldBe("{\"bestMetric\":\"accuracy\"}");
                experiment.Model.Metrics.Count.ShouldBe(2);
                experiment.Model.FeatureImportances.Count.ShouldBe(2);
                experiment.Model.FeatureImportances.OrderBy(item => item.Rank).First().DatasetColumn.Name.ShouldBe("age");
            });
        }

        [Fact]
        public async Task Should_Persist_A_Failed_Experiment_Without_A_Model()
        {
            long experimentId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = AbpSession.GetTenantId(),
                    Name = "failed-ml-dataset",
                    SourceFormat = DatasetFormat.Json,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "failed-ml.json"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                await context.SaveChangesAsync();

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    Status = MLExperimentStatus.Failed,
                    TaskType = MLTaskType.Regression,
                    AlgorithmKey = "linear_regression",
                    TrainingConfigurationJson = "{\"algorithm\":\"linear_regression\"}",
                    ExecutedAt = DateTime.UtcNow,
                    FailureMessage = "Training failed because the target column contained only null values."
                }).Entity;

                await context.SaveChangesAsync();
                experimentId = experiment.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var experiment = await context.MLExperiments
                    .Include(item => item.Model)
                    .SingleAsync(item => item.Id == experimentId);

                experiment.Status.ShouldBe(MLExperimentStatus.Failed);
                experiment.FailureMessage.ShouldNotBeNull();
                experiment.Model.ShouldBeNull();
            });
        }

        [Fact]
        public async Task Should_Support_Tenant_Scoped_Ml_Queries()
        {
            long experimentId = 0;
            long modelId = 0;
            int secondTenantId = 0;

            await UsingDbContextAsync((int?)null, async context =>
            {
                var tenant = new Tenant("mlexperimenttenant", "ML Experiment Tenant")
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
                    Name = "tenant-ml-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "tenant-ml.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 100
                }).Entity;

                await context.SaveChangesAsync();

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Status = MLExperimentStatus.Completed,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "xgboost",
                    TrainingConfigurationJson = "{\"algorithm\":\"xgboost\"}",
                    ExecutedAt = DateTime.UtcNow
                }).Entity;

                await context.SaveChangesAsync();

                var model = context.MLModels.Add(new MLModel
                {
                    TenantId = 1,
                    MLExperimentId = experiment.Id,
                    ModelType = "xgboost"
                }).Entity;

                await context.SaveChangesAsync();
                experimentId = experiment.Id;
                modelId = model.Id;
            });

            var tenantExperimentCount = await UsingDbContextAsync((int?)null, async context =>
                await context.MLExperiments.CountAsync(item => item.Id == experimentId && item.TenantId == 1));

            var secondTenantExperimentCount = await UsingDbContextAsync((int?)null, async context =>
                await context.MLExperiments.CountAsync(item => item.Id == experimentId && item.TenantId == secondTenantId));

            var tenantModelCount = await UsingDbContextAsync((int?)null, async context =>
                await context.MLModels.CountAsync(item => item.Id == modelId && item.TenantId == 1));

            var secondTenantModelCount = await UsingDbContextAsync((int?)null, async context =>
                await context.MLModels.CountAsync(item => item.Id == modelId && item.TenantId == secondTenantId));

            tenantExperimentCount.ShouldBe(1);
            secondTenantExperimentCount.ShouldBe(0);
            tenantModelCount.ShouldBe(1);
            secondTenantModelCount.ShouldBe(0);
        }

        [Fact]
        public void Should_Define_Expected_Ml_Unique_Indexes_In_Model()
        {
            UsingDbContext(context =>
            {
                context.Model.FindEntityType(typeof(MLModel)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLModel.MLExperimentId) }))
                    .IsUnique.ShouldBeTrue();

                context.Model.FindEntityType(typeof(MLExperimentFeature)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLExperimentFeature.MLExperimentId), nameof(MLExperimentFeature.DatasetColumnId) }))
                    .IsUnique.ShouldBeTrue();

                context.Model.FindEntityType(typeof(MLExperimentFeature)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLExperimentFeature.MLExperimentId), nameof(MLExperimentFeature.Ordinal) }))
                    .IsUnique.ShouldBeTrue();

                context.Model.FindEntityType(typeof(MLModelMetric)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLModelMetric.MLModelId), nameof(MLModelMetric.MetricName) }))
                    .IsUnique.ShouldBeTrue();

                context.Model.FindEntityType(typeof(MLModelFeatureImportance)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLModelFeatureImportance.MLModelId), nameof(MLModelFeatureImportance.DatasetColumnId) }))
                    .IsUnique.ShouldBeTrue();

                context.Model.FindEntityType(typeof(MLExperiment)).GetIndexes()
                    .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(MLExperiment.TenantId), nameof(MLExperiment.DatasetVersionId), nameof(MLExperiment.Status) }))
                    .IsUnique.ShouldBeFalse();
            });
        }

        [Fact]
        public void Should_Define_Expected_Ml_Delete_Behaviors_In_Model()
        {
            UsingDbContext(context =>
            {
                context.Model.FindEntityType(typeof(MLExperiment)).GetForeignKeys()
                    .Single(key => key.Properties.Single().Name == nameof(MLExperiment.DatasetVersionId) && key.PrincipalEntityType.ClrType == typeof(DatasetVersion))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);

                context.Model.FindEntityType(typeof(MLModel)).GetForeignKeys()
                    .Single(key => key.Properties.Single().Name == nameof(MLModel.MLExperimentId) && key.PrincipalEntityType.ClrType == typeof(MLExperiment))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);

                context.Model.FindEntityType(typeof(MLExperiment)).GetForeignKeys()
                    .Single(key => key.Properties.Single().Name == nameof(MLExperiment.TargetDatasetColumnId) && key.PrincipalEntityType.ClrType == typeof(DatasetColumn))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);

                context.Model.FindEntityType(typeof(MLExperimentFeature)).GetForeignKeys()
                    .Single(key => key.Properties.Single().Name == nameof(MLExperimentFeature.DatasetColumnId) && key.PrincipalEntityType.ClrType == typeof(DatasetColumn))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);

                context.Model.FindEntityType(typeof(MLModelFeatureImportance)).GetForeignKeys()
                    .Single(key => key.Properties.Single().Name == nameof(MLModelFeatureImportance.DatasetColumnId) && key.PrincipalEntityType.ClrType == typeof(DatasetColumn))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
            });
        }
    }
}
