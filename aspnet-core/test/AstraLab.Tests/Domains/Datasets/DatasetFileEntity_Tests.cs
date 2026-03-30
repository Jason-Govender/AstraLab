using System.Linq;
using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Domains.Datasets
{
    public class DatasetFileEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Dataset_File_Should_Roundtrip_For_Dataset_Version()
        {
            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "dataset-file-roundtrip",
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
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                await context.SaveChangesAsync();

                context.DatasetFiles.Add(new DatasetFile
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = "local-filesystem",
                    StorageKey = "tenants/1/datasets/1/versions/1/raw/abc123.csv",
                    OriginalFileName = "dataset.csv",
                    ContentType = "text/csv",
                    SizeBytes = 512,
                    ChecksumSha256 = new string('a', DatasetFile.ChecksumSha256Length)
                });

                await context.SaveChangesAsync();
            });

            await UsingDbContextAsync(async context =>
            {
                var datasetFile = await context.DatasetFiles
                    .Include(item => item.DatasetVersion)
                    .SingleAsync();

                datasetFile.StorageProvider.ShouldBe("local-filesystem");
                datasetFile.StorageKey.ShouldBe("tenants/1/datasets/1/versions/1/raw/abc123.csv");
                datasetFile.OriginalFileName.ShouldBe("dataset.csv");
                datasetFile.ContentType.ShouldBe("text/csv");
                datasetFile.DatasetVersion.VersionType.ShouldBe(DatasetVersionType.Raw);
            });
        }

        [Fact]
        public void Dataset_File_Model_Should_Define_Expected_Indexes_And_Cascade_Delete()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(DatasetFile));
                var indexes = entityType.GetIndexes().ToList();

                indexes.Any(index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(DatasetFile.DatasetVersionId) }))
                    .ShouldBeTrue();

                indexes.Any(index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(DatasetFile.StorageProvider), nameof(DatasetFile.StorageKey) }))
                    .ShouldBeTrue();

                var foreignKey = entityType.GetForeignKeys().Single();
                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }
    }
}
