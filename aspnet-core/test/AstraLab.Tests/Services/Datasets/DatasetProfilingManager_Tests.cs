using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.AI;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Profiling;
using AstraLab.Services.Datasets.Storage;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetProfilingManager_Tests : AstraLabTestBase
    {
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IDatasetProfiler _datasetProfiler;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IDatasetProfilingManager _datasetProfilingManager;

        public DatasetProfilingManager_Tests()
        {
            _rawDatasetStorage = Substitute.For<IRawDatasetStorage>();
            _datasetProfiler = Substitute.For<IDatasetProfiler>();
            _backgroundJobManager = Substitute.For<IBackgroundJobManager>();

            LocalIocManager.IocContainer.Register(
                Component.For<IRawDatasetStorage>().Instance(_rawDatasetStorage).IsDefault().LifestyleSingleton(),
                Component.For<IDatasetProfiler>().Instance(_datasetProfiler).IsDefault().LifestyleSingleton(),
                Component.For<IBackgroundJobManager>().Instance(_backgroundJobManager).IsDefault().LifestyleSingleton());

            _datasetProfilingManager = Resolve<IDatasetProfilingManager>();
        }

        [Fact]
        public async Task ProfileAsync_Should_Enqueue_An_Automatic_Insight_Job_After_Successful_Profiling()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "profiling-success-dataset",
                    Description = "profiling success dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.UserId.Value,
                    OriginalFileName = "profiling-success.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    RowCount = 5,
                    ColumnCount = 1,
                    SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}",
                    SizeBytes = 256
                }).Entity;

                context.SaveChanges();

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "amount",
                    DataType = "decimal",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = "local",
                    StorageKey = "versions/1/raw/test.csv",
                    OriginalFileName = "test.csv",
                    SizeBytes = 128,
                    ChecksumSha256 = new string('a', 64)
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            _rawDatasetStorage.OpenReadAsync(Arg.Any<OpenReadRawDatasetFileRequest>())
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));
            _datasetProfiler.ProfileAsync(Arg.Any<ProfileDatasetVersionRequest>())
                .Returns(Task.FromResult(new ProfileDatasetVersionResult
                {
                    RowCount = 5,
                    DuplicateRowCount = 1,
                    DataHealthScore = 84.5m,
                    SummaryJson = "{\"totalNullCount\":1,\"overallNullPercentage\":20.0,\"totalAnomalyCount\":0,\"overallAnomalyPercentage\":0.0}",
                    Columns = new[]
                    {
                        new ProfiledDatasetColumnResult
                        {
                            DatasetColumnId = UsingDbContext(context => context.DatasetColumns.Single(item => item.DatasetVersionId == datasetVersionId).Id),
                            InferredDataType = "decimal",
                            NullCount = 1,
                            DistinctCount = 4,
                            StatisticsJson = "{\"nullPercentage\":20.0,\"anomalyCount\":0,\"anomalyPercentage\":0.0,\"hasAnomalies\":false}"
                        }
                    }
                }));
            _backgroundJobManager.EnqueueAsync<GenerateAutomaticDatasetInsightJob, GenerateAutomaticDatasetInsightJobArgs>(
                    Arg.Any<GenerateAutomaticDatasetInsightJobArgs>(),
                    Arg.Any<BackgroundJobPriority>(),
                    Arg.Any<System.TimeSpan?>())
                .Returns(Task.FromResult("job-1"));

            await _datasetProfilingManager.ProfileAsync(datasetVersionId);

            await _backgroundJobManager.Received(1).EnqueueAsync<GenerateAutomaticDatasetInsightJob, GenerateAutomaticDatasetInsightJobArgs>(
                Arg.Is<GenerateAutomaticDatasetInsightJobArgs>(item =>
                    item.DatasetVersionId == datasetVersionId &&
                    item.TenantId == 1 &&
                    item.OwnerUserId == AbpSession.UserId.Value &&
                    item.DatasetProfileId > 0),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<System.TimeSpan?>());
        }

        [Fact]
        public async Task ProfileAsync_Should_Not_Enqueue_An_Automatic_Insight_Job_When_Profiling_Fails()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "profiling-failure-dataset",
                    Description = "profiling failure dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.UserId.Value,
                    OriginalFileName = "profiling-failure.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    RowCount = 5,
                    ColumnCount = 1,
                    SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}",
                    SizeBytes = 256
                }).Entity;

                context.SaveChanges();

                context.DatasetColumns.Add(new DatasetColumn
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Name = "amount",
                    DataType = "decimal",
                    IsDataTypeInferred = true,
                    Ordinal = 1
                });

                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = "local",
                    StorageKey = "versions/2/raw/test.csv",
                    OriginalFileName = "test.csv",
                    SizeBytes = 128,
                    ChecksumSha256 = new string('b', 64)
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            _rawDatasetStorage.OpenReadAsync(Arg.Any<OpenReadRawDatasetFileRequest>())
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));
            _datasetProfiler.ProfileAsync(Arg.Any<ProfileDatasetVersionRequest>())
                .Returns<Task<ProfileDatasetVersionResult>>(_ => throw new InvalidDataException("Profile failure."));

            await Should.ThrowAsync<InvalidDataException>(() => _datasetProfilingManager.ProfileAsync(datasetVersionId));

            await _backgroundJobManager.DidNotReceive().EnqueueAsync<GenerateAutomaticDatasetInsightJob, GenerateAutomaticDatasetInsightJobArgs>(
                Arg.Any<GenerateAutomaticDatasetInsightJobArgs>(),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<System.TimeSpan?>());
        }
    }
}
