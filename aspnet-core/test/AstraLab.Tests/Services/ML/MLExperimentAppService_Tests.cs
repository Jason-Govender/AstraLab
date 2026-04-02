using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.BackgroundJobs;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.MultiTenancy;
using AstraLab.Services.AI;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Dto;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.ML
{
    public class MLExperimentAppService_Tests : AstraLabTestBase
    {
        private readonly IMLExperimentAppService _mlExperimentAppService;
        private readonly IMLExperimentExecutionManager _mlExperimentExecutionManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IMLJobDispatcher _mlJobDispatcher;

        public MLExperimentAppService_Tests()
        {
            _backgroundJobManager = Substitute.For<IBackgroundJobManager>();
            _mlJobDispatcher = Resolve<IMLJobDispatcher>();
            LocalIocManager.IocContainer.Register(
                Component.For<IBackgroundJobManager>()
                    .Instance(_backgroundJobManager)
                    .IsDefault()
                    .LifestyleSingleton());
            _mlExperimentAppService = Resolve<IMLExperimentAppService>();
            _mlExperimentExecutionManager = Resolve<IMLExperimentExecutionManager>();
            _mlJobDispatcher.DispatchAsync(Arg.Any<DispatchMlExperimentRequest>()).Returns(Task.CompletedTask);
            _backgroundJobManager.EnqueueAsync<GenerateAutomaticExperimentInsightJob, GenerateAutomaticExperimentInsightJobArgs>(
                    Arg.Any<GenerateAutomaticExperimentInsightJobArgs>(),
                    Arg.Any<BackgroundJobPriority>(),
                    Arg.Any<TimeSpan?>())
                .Returns(Task.FromResult("ml-ai-job-1"));
        }

        [Fact]
        public async Task CreateAsync_Should_Create_And_Dispatch_A_Classification_Experiment()
        {
            var seeded = SeedExperimentDataset();

            var output = await _mlExperimentAppService.CreateAsync(new CreateMlExperimentRequest
            {
                DatasetVersionId = seeded.DatasetVersionId,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = "logistic_regression",
                FeatureDatasetColumnIds = new System.Collections.Generic.List<long>
                {
                    seeded.FeatureColumnId,
                    seeded.SecondFeatureColumnId
                },
                TargetDatasetColumnId = seeded.TargetColumnId,
                TrainingConfigurationJson = "{\"testSize\":0.25}"
            });

            output.Status.ShouldBe(MLExperimentStatus.Running);
            output.TaskType.ShouldBe(MLTaskType.Classification);
            output.AlgorithmKey.ShouldBe("logistic_regression");
            output.Features.Select(item => item.DatasetColumnId).ShouldBe(new[]
            {
                seeded.FeatureColumnId,
                seeded.SecondFeatureColumnId
            });
            output.TargetDatasetColumnId.ShouldBe(seeded.TargetColumnId);
            output.StartedAtUtc.ShouldNotBeNull();
            output.DispatchErrorMessage.ShouldBeNull();

            await _mlJobDispatcher.Received(1).DispatchAsync(Arg.Is<DispatchMlExperimentRequest>(item =>
                item.ExperimentId == output.Id &&
                item.DatasetVersionId == seeded.DatasetVersionId &&
                item.DatasetFormat == "csv" &&
                item.TaskType == "classification" &&
                item.AlgorithmKey == "logistic_regression" &&
                item.FeatureColumns.Count == 2 &&
                item.TargetColumn != null &&
                item.TargetColumn.DatasetColumnId == seeded.TargetColumnId));

            await UsingDbContextAsync(async context =>
            {
                var experiment = context.MLExperiments.Single(item => item.Id == output.Id);
                experiment.Status.ShouldBe(MLExperimentStatus.Running);
                experiment.TaskType.ShouldBe(MLTaskType.Classification);
                experiment.AlgorithmKey.ShouldBe("logistic_regression");
                experiment.StartedAtUtc.ShouldNotBeNull();
                experiment.DispatchErrorMessage.ShouldBeNull();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task CreateAsync_Should_Reject_A_Supervised_Experiment_Without_A_Target()
        {
            var seeded = SeedExperimentDataset();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _mlExperimentAppService.CreateAsync(new CreateMlExperimentRequest
                {
                    DatasetVersionId = seeded.DatasetVersionId,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "logistic_regression",
                    FeatureDatasetColumnIds = new System.Collections.Generic.List<long>
                    {
                        seeded.FeatureColumnId
                    }
                }));

            exception.Message.ShouldBe("The selected ML task type requires a target column.");
            await _mlJobDispatcher.DidNotReceiveWithAnyArgs().DispatchAsync(default);
        }

        [Fact]
        public async Task CreateAsync_Should_Reject_A_Target_For_Clustering()
        {
            var seeded = SeedExperimentDataset();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _mlExperimentAppService.CreateAsync(new CreateMlExperimentRequest
                {
                    DatasetVersionId = seeded.DatasetVersionId,
                    TaskType = MLTaskType.Clustering,
                    AlgorithmKey = "kmeans",
                    FeatureDatasetColumnIds = new System.Collections.Generic.List<long>
                    {
                        seeded.FeatureColumnId,
                        seeded.SecondFeatureColumnId
                    },
                    TargetDatasetColumnId = seeded.TargetColumnId
                }));

            exception.Message.ShouldBe("The selected ML task type does not support a target column.");
            await _mlJobDispatcher.DidNotReceiveWithAnyArgs().DispatchAsync(default);
        }

        [Fact]
        public async Task CreateAsync_Should_Leave_The_Experiment_Pending_When_Dispatch_Fails()
        {
            var seeded = SeedExperimentDataset();
            _mlJobDispatcher
                .When(item => item.DispatchAsync(Arg.Any<DispatchMlExperimentRequest>()))
                .Do(_ => throw new InvalidOperationException("The ML executor is offline."));

            var output = await _mlExperimentAppService.CreateAsync(new CreateMlExperimentRequest
            {
                DatasetVersionId = seeded.DatasetVersionId,
                TaskType = MLTaskType.Regression,
                AlgorithmKey = "linear_regression",
                FeatureDatasetColumnIds = new System.Collections.Generic.List<long>
                {
                    seeded.FeatureColumnId,
                    seeded.SecondFeatureColumnId
                },
                TargetDatasetColumnId = seeded.TargetColumnId,
                TrainingConfigurationJson = "{\"fitIntercept\":true}"
            });

            output.Status.ShouldBe(MLExperimentStatus.Pending);
            output.StartedAtUtc.ShouldBeNull();
            output.DispatchErrorMessage.ShouldBe("The ML executor is offline.");

            await UsingDbContextAsync(async context =>
            {
                var experiment = context.MLExperiments.Single(item => item.Id == output.Id);
                experiment.Status.ShouldBe(MLExperimentStatus.Pending);
                experiment.StartedAtUtc.ShouldBeNull();
                experiment.DispatchErrorMessage.ShouldBe("The ML executor is offline.");
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task CancelAsync_Should_Cancel_A_Pending_Experiment()
        {
            var seeded = SeedExperimentDataset();
            var experimentId = SeedExperiment(seeded, MLExperimentStatus.Pending, MLTaskType.Classification, "logistic_regression");

            var output = await _mlExperimentAppService.CancelAsync(new EntityDto<long>(experimentId));

            output.Status.ShouldBe(MLExperimentStatus.Cancelled);
            output.CompletedAtUtc.ShouldNotBeNull();
        }

        [Fact]
        public async Task RetryAsync_Should_Create_A_New_Experiment_From_A_Failed_Experiment()
        {
            var seeded = SeedExperimentDataset();
            var failedExperimentId = SeedExperiment(seeded, MLExperimentStatus.Failed, MLTaskType.Classification, "random_forest_classifier");

            var output = await _mlExperimentAppService.RetryAsync(new EntityDto<long>(failedExperimentId));

            output.Id.ShouldNotBe(failedExperimentId);
            output.Status.ShouldBe(MLExperimentStatus.Running);
            output.AlgorithmKey.ShouldBe("random_forest_classifier");
            output.Features.Count.ShouldBe(2);

            await UsingDbContextAsync(async context =>
            {
                context.MLExperiments.Count().ShouldBe(2);
                context.MLExperiments.Count(item => item.Status == MLExperimentStatus.Running).ShouldBe(1);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task CompleteAsync_Should_Persist_A_Model_Once_When_A_Callback_Is_Replayed()
        {
            var seeded = SeedExperimentDataset();
            var experimentId = SeedExperiment(
                seeded,
                MLExperimentStatus.Running,
                MLTaskType.Classification,
                "random_forest_classifier",
                startedAtUtc: new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc));

            var callback = new CompleteMlExperimentCallbackRequest
            {
                ExperimentId = experimentId,
                StartedAtUtc = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 4, 1, 9, 2, 0, DateTimeKind.Utc),
                ModelType = "random_forest_classifier",
                ArtifactStorageProvider = "local-filesystem",
                ArtifactStorageKey = "ml-artifacts/tenant-1/experiment-1/model.joblib",
                PerformanceSummaryJson = "{\"primaryMetric\":\"accuracy\"}",
                WarningsJson = "[\"train_test_split\"]",
                Metrics = new System.Collections.Generic.List<MlExperimentCompletionMetricDto>
                {
                    new MlExperimentCompletionMetricDto
                    {
                        MetricName = "accuracy",
                        MetricValue = 0.91m
                    },
                    new MlExperimentCompletionMetricDto
                    {
                        MetricName = "f1",
                        MetricValue = 0.89m
                    }
                },
                FeatureImportances = new System.Collections.Generic.List<MlExperimentCompletionFeatureImportanceDto>
                {
                    new MlExperimentCompletionFeatureImportanceDto
                    {
                        DatasetColumnId = seeded.FeatureColumnId,
                        ImportanceScore = 0.65m,
                        Rank = 1
                    },
                    new MlExperimentCompletionFeatureImportanceDto
                    {
                        DatasetColumnId = seeded.SecondFeatureColumnId,
                        ImportanceScore = 0.35m,
                        Rank = 2
                    }
                }
            };

            await _mlExperimentExecutionManager.CompleteAsync(callback);
            await _mlExperimentExecutionManager.CompleteAsync(callback);

            await UsingDbContextAsync(async context =>
            {
                var experiment = context.MLExperiments.Single(item => item.Id == experimentId);
                var model = context.MLModels.Single(item => item.MLExperimentId == experimentId);

                experiment.Status.ShouldBe(MLExperimentStatus.Completed);
                experiment.CompletedAtUtc.ShouldBe(callback.CompletedAtUtc);
                experiment.WarningsJson.ShouldBe("[\"train_test_split\"]");

                model.ModelType.ShouldBe("random_forest_classifier");
                model.ArtifactStorageProvider.ShouldBe("local-filesystem");
                model.ArtifactStorageKey.ShouldBe("ml-artifacts/tenant-1/experiment-1/model.joblib");

                context.MLModels.Count(item => item.MLExperimentId == experimentId).ShouldBe(1);
                context.MLModelMetrics.Count(item => item.MLModelId == model.Id).ShouldBe(2);
                context.MLModelFeatureImportances.Count(item => item.MLModelId == model.Id).ShouldBe(2);
                await Task.CompletedTask;
            });

            await _backgroundJobManager.Received(1).EnqueueAsync<GenerateAutomaticExperimentInsightJob, GenerateAutomaticExperimentInsightJobArgs>(
                Arg.Is<GenerateAutomaticExperimentInsightJobArgs>(item =>
                    item.MLExperimentId == experimentId &&
                    item.DatasetVersionId == seeded.DatasetVersionId &&
                    item.TenantId == 1 &&
                    item.OwnerUserId == AbpSession.GetUserId()),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<TimeSpan?>());
        }

        [Fact]
        public async Task FailAsync_Should_Mark_An_Experiment_As_Failed_Without_Creating_A_Model()
        {
            var seeded = SeedExperimentDataset();
            var experimentId = SeedExperiment(
                seeded,
                MLExperimentStatus.Running,
                MLTaskType.Regression,
                "linear_regression",
                startedAtUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));

            await _mlExperimentExecutionManager.FailAsync(new FailMlExperimentCallbackRequest
            {
                ExperimentId = experimentId,
                StartedAtUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 4, 1, 10, 1, 0, DateTimeKind.Utc),
                FailureMessage = "The target column contained only null values.",
                WarningsJson = "[\"validation\"]"
            });

            await UsingDbContextAsync(async context =>
            {
                var experiment = context.MLExperiments.Single(item => item.Id == experimentId);
                experiment.Status.ShouldBe(MLExperimentStatus.Failed);
                experiment.FailureMessage.ShouldBe("The target column contained only null values.");
                experiment.CompletedAtUtc.ShouldBe(new DateTime(2026, 4, 1, 10, 1, 0, DateTimeKind.Utc));
                experiment.WarningsJson.ShouldBe("[\"validation\"]");
                context.MLModels.Count(item => item.MLExperimentId == experimentId).ShouldBe(0);
                await Task.CompletedTask;
            });

            await _backgroundJobManager.DidNotReceive().EnqueueAsync<GenerateAutomaticExperimentInsightJob, GenerateAutomaticExperimentInsightJobArgs>(
                Arg.Any<GenerateAutomaticExperimentInsightJobArgs>(),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<TimeSpan?>());
        }

        [Fact]
        public async Task GetAsync_Should_Hide_Experiments_From_Other_Owners_In_The_Same_Tenant()
        {
            var experimentId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "hidden-ml-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.GetUserId() + 100,
                    OriginalFileName = "hidden.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                context.SaveChanges();

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Status = MLExperimentStatus.Pending,
                    TaskType = MLTaskType.Clustering,
                    AlgorithmKey = "kmeans",
                    TrainingConfigurationJson = "{}",
                    ExecutedAt = DateTime.UtcNow
                }).Entity;

                context.SaveChanges();
                return experiment.Id;
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _mlExperimentAppService.GetAsync(new EntityDto<long>(experimentId)));

            exception.Message.ShouldBe("The requested ML experiment could not be found.");
        }

        private MlExperimentSeedResult SeedExperimentDataset()
        {
            return UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "ml-service-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.GetUserId(),
                    OriginalFileName = "ml-service.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    RowCount = 25,
                    ColumnCount = 3,
                    SizeBytes = 1024
                }).Entity;

                context.SaveChanges();

                var firstFeature = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "age",
                    DataType = "integer",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                }).Entity;

                var secondFeature = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "income",
                    DataType = "decimal",
                    IsDataTypeInferred = true,
                    Ordinal = 2
                }).Entity;

                var target = context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "will_buy",
                    DataType = "boolean",
                    IsDataTypeInferred = true,
                    Ordinal = 3
                }).Entity;

                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = "local-filesystem",
                    StorageKey = "tenants/1/datasets/ml-service/versions/1/raw/ml-service.csv",
                    OriginalFileName = "ml-service.csv",
                    ContentType = "text/csv",
                    SizeBytes = 1024,
                    ChecksumSha256 = new string('b', 64)
                });

                context.SaveChanges();
                return new MlExperimentSeedResult(dataset.Id, datasetVersion.Id, firstFeature.Id, secondFeature.Id, target.Id);
            });
        }

        private long SeedExperiment(
            MlExperimentSeedResult seeded,
            MLExperimentStatus status,
            MLTaskType taskType,
            string algorithmKey,
            DateTime? startedAtUtc = null)
        {
            return UsingDbContext(context =>
            {
                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = seeded.DatasetVersionId,
                    TargetDatasetColumnId = seeded.TargetColumnId,
                    Status = status,
                    TaskType = taskType,
                    AlgorithmKey = algorithmKey,
                    TrainingConfigurationJson = "{}",
                    ExecutedAt = new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
                    StartedAtUtc = startedAtUtc,
                    CompletedAtUtc = status == MLExperimentStatus.Failed
                        ? new DateTime(2026, 4, 1, 8, 5, 0, DateTimeKind.Utc)
                        : null,
                    FailureMessage = status == MLExperimentStatus.Failed
                        ? "The previous run failed."
                        : null
                }).Entity;

                context.SaveChanges();

                context.MLExperimentFeatures.Add(new MLExperimentFeature
                {
                    TenantId = 1,
                    MLExperimentId = experiment.Id,
                    DatasetColumnId = seeded.FeatureColumnId,
                    Ordinal = 1
                });

                context.MLExperimentFeatures.Add(new MLExperimentFeature
                {
                    TenantId = 1,
                    MLExperimentId = experiment.Id,
                    DatasetColumnId = seeded.SecondFeatureColumnId,
                    Ordinal = 2
                });

                context.SaveChanges();
                return experiment.Id;
            });
        }

        private class MlExperimentSeedResult
        {
            public MlExperimentSeedResult(
                long datasetId,
                long datasetVersionId,
                long featureColumnId,
                long secondFeatureColumnId,
                long targetColumnId)
            {
                DatasetId = datasetId;
                DatasetVersionId = datasetVersionId;
                FeatureColumnId = featureColumnId;
                SecondFeatureColumnId = secondFeatureColumnId;
                TargetColumnId = targetColumnId;
            }

            public long DatasetId { get; }

            public long DatasetVersionId { get; }

            public long FeatureColumnId { get; }

            public long SecondFeatureColumnId { get; }

            public long TargetColumnId { get; }
        }
    }
}
