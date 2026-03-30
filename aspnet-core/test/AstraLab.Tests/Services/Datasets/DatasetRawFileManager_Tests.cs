using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.Datasets.Storage;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetRawFileManager_Tests : AstraLabTestBase
    {
        private readonly IDatasetRawFileManager _datasetRawFileManager;

        public DatasetRawFileManager_Tests()
        {
            _datasetRawFileManager = Resolve<IDatasetRawFileManager>();
        }

        [Fact]
        public async Task StoreForVersionAsync_Should_Persist_Raw_File_Record_With_Logical_Reference()
        {
            var datasetVersionDetails = await CreateDatasetVersionAsync(DatasetVersionType.Raw);

            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
            {
                var result = await _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                {
                    DatasetId = datasetVersionDetails.DatasetId,
                    DatasetVersionId = datasetVersionDetails.DatasetVersionId,
                    OriginalFileName = "customers.csv",
                    ContentType = "text/csv",
                    Content = content
                });

                result.StorageProvider.ShouldBe("local-filesystem");
                result.StorageKey.ShouldStartWith($"tenants/1/datasets/{datasetVersionDetails.DatasetId}/versions/{datasetVersionDetails.DatasetVersionId}/raw/");
                Path.IsPathRooted(result.StorageKey).ShouldBeFalse();
                result.SizeBytes.ShouldBeGreaterThan(0);
                result.ChecksumSha256.Length.ShouldBe(DatasetFile.ChecksumSha256Length);
            }

            await UsingDbContextAsync(async context =>
            {
                var datasetFile = context.DatasetFiles.Single();

                datasetFile.TenantId.ShouldBe(1);
                datasetFile.DatasetVersionId.ShouldBe(datasetVersionDetails.DatasetVersionId);
                datasetFile.StorageProvider.ShouldBe("local-filesystem");
                Path.IsPathRooted(datasetFile.StorageKey).ShouldBeFalse();
                datasetFile.OriginalFileName.ShouldBe("customers.csv");
                datasetFile.ContentType.ShouldBe("text/csv");
                datasetFile.SizeBytes.ShouldBeGreaterThan(0);
                datasetFile.ChecksumSha256.Length.ShouldBe(DatasetFile.ChecksumSha256Length);

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StoreForVersionAsync_Should_Reject_A_Second_Raw_File_For_The_Same_Version()
        {
            var datasetVersionDetails = await CreateDatasetVersionAsync(DatasetVersionType.Raw);

            using (var firstContent = new MemoryStream(Encoding.UTF8.GetBytes("a,b\n1,2\n")))
            {
                await _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                {
                    DatasetId = datasetVersionDetails.DatasetId,
                    DatasetVersionId = datasetVersionDetails.DatasetVersionId,
                    OriginalFileName = "first.csv",
                    Content = firstContent
                });
            }

            using (var secondContent = new MemoryStream(Encoding.UTF8.GetBytes("a,b\n3,4\n")))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                    _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                    {
                        DatasetId = datasetVersionDetails.DatasetId,
                        DatasetVersionId = datasetVersionDetails.DatasetVersionId,
                        OriginalFileName = "second.csv",
                        Content = secondContent
                    }));

                exception.Message.ShouldBe("A raw dataset file already exists for the specified dataset version.");
            }
        }

        [Fact]
        public async Task StoreForVersionAsync_Should_Reject_Processed_Dataset_Versions()
        {
            var datasetVersionDetails = await CreateDatasetVersionAsync(DatasetVersionType.Processed);

            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("a,b\n1,2\n")))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                    _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                    {
                        DatasetId = datasetVersionDetails.DatasetId,
                        DatasetVersionId = datasetVersionDetails.DatasetVersionId,
                        OriginalFileName = "processed.csv",
                        Content = content
                    }));

                exception.Message.ShouldBe("Only raw dataset versions can store immutable raw files.");
            }
        }

        [Fact]
        public async Task StoreForVersionAsync_Should_Reject_Dataset_Versions_From_Other_Tenants()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("dataset-file-other", "Dataset File Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantVersionDetails = UsingDbContext(otherTenantId, context =>
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
                    SizeBytes = 256
                }).Entity;

                context.SaveChanges();
                return new DatasetVersionDetails(dataset.Id, datasetVersion.Id);
            });

            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("a,b\n1,2\n")))
            {
                await Should.ThrowAsync<EntityNotFoundException>(() =>
                    _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                    {
                        DatasetId = otherTenantVersionDetails.DatasetId,
                        DatasetVersionId = otherTenantVersionDetails.DatasetVersionId,
                        OriginalFileName = "other.csv",
                        Content = content
                    }));
            }
        }

        private async Task<DatasetVersionDetails> CreateDatasetVersionAsync(DatasetVersionType versionType)
        {
            return await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = $"dataset-{versionType.ToString().ToLowerInvariant()}",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = 1,
                    OriginalFileName = "dataset.csv"
                }).Entity;

                await context.SaveChangesAsync();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = versionType,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 256
                }).Entity;

                await context.SaveChangesAsync();
                return new DatasetVersionDetails(dataset.Id, datasetVersion.Id);
            });
        }

        private class DatasetVersionDetails
        {
            public DatasetVersionDetails(long datasetId, long datasetVersionId)
            {
                DatasetId = datasetId;
                DatasetVersionId = datasetVersionId;
            }

            public long DatasetId { get; }

            public long DatasetVersionId { get; }
        }
    }
}
