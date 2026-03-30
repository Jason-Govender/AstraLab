using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using NSubstitute;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.Datasets;
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

        [Fact]
        public async Task StoreForVersionAsync_Should_Reject_Dataset_Versions_From_Other_Owner_In_Same_Tenant()
        {
            var otherOwnerVersionDetails = UsingDbContext(1, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "other-owner-dataset",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Uploaded,
                    OwnerUserId = AbpSession.GetUserId() + 200,
                    OriginalFileName = "other-owner.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
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
                        DatasetId = otherOwnerVersionDetails.DatasetId,
                        DatasetVersionId = otherOwnerVersionDetails.DatasetVersionId,
                        OriginalFileName = "other-owner.csv",
                        Content = content
                    }));
            }
        }

        [Fact]
        public async Task StoreForVersionAsync_Should_Delete_Stored_File_When_Database_Persistence_Fails()
        {
            var datasetVersionDetails = await CreateDatasetVersionAsync(DatasetVersionType.Raw);
            var contentBytes = Encoding.UTF8.GetBytes("id,name\n1,Alice\n");
            var checksum = Convert.ToHexString(SHA256.HashData(contentBytes)).ToLowerInvariant();
            var storageKey = $"tenants/1/datasets/{datasetVersionDetails.DatasetId}/versions/{datasetVersionDetails.DatasetVersionId}/raw/{checksum}.csv";

            var storage = Substitute.For<IRawDatasetStorage>();
            storage.StoreAsync(Arg.Any<StoreRawDatasetFileRequest>())
                .Returns(Task.FromResult(new StoredRawDatasetFileResult
                {
                    StorageProvider = "local-filesystem",
                    StorageKey = storageKey,
                    OriginalFileName = "customers.csv",
                    SizeBytes = contentBytes.Length,
                    ChecksumSha256 = checksum
                }));

            var datasetFileRepository = Substitute.For<IRepository<DatasetFile, long>>();
            datasetFileRepository.InsertAsync(Arg.Any<DatasetFile>())
                .Returns<Task<DatasetFile>>(_ => throw new IOException("Simulated dataset file persistence failure."));

            var manager = new DatasetRawFileManager(
                Resolve<IAbpSession>(),
                Resolve<IUnitOfWorkManager>(),
                datasetFileRepository,
                Resolve<IDatasetOwnershipAccessChecker>(),
                storage);

            using (var content = new MemoryStream(contentBytes))
            {
                await Should.ThrowAsync<System.Exception>(() =>
                    manager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                    {
                        DatasetId = datasetVersionDetails.DatasetId,
                        DatasetVersionId = datasetVersionDetails.DatasetVersionId,
                        OriginalFileName = "customers.csv",
                        Content = content
                    }));
            }

            await storage.Received(1).DeleteAsync(Arg.Is<DeleteRawDatasetFileRequest>(item =>
                item.StorageProvider == "local-filesystem" &&
                item.StorageKey == storageKey));
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
                    OwnerUserId = AbpSession.GetUserId(),
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
