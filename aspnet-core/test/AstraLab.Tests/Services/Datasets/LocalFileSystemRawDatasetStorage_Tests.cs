using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Web.Core.Datasets.Storage;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class LocalFileSystemRawDatasetStorage_Tests
    {
        [Fact]
        public async Task StoreAsync_Should_Write_File_Using_Expected_Logical_Key_And_Clear_Temp_Files()
        {
            var rawRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "LocalStorage", Path.GetRandomFileName());
            Directory.CreateDirectory(rawRootPath);

            try
            {
                var storage = new LocalFileSystemRawDatasetStorage(new DatasetStorageOptions
                {
                    RawRootPath = rawRootPath
                });

                using (var content = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
                {
                    var result = await storage.StoreAsync(new StoreRawDatasetFileRequest
                    {
                        TenantId = 4,
                        DatasetId = 8,
                        DatasetVersionId = 15,
                        OriginalFileName = "customers.csv",
                        ContentType = "text/csv",
                        Content = content
                    });

                    result.StorageProvider.ShouldBe(LocalFileSystemRawDatasetStorage.ProviderName);
                    result.StorageKey.ShouldStartWith("tenants/4/datasets/8/versions/15/raw/");
                    result.StorageKey.ShouldEndWith(".csv");
                    Path.IsPathRooted(result.StorageKey).ShouldBeFalse();

                    var persistedFilePath = Path.Combine(rawRootPath, result.StorageKey.Replace('/', Path.DirectorySeparatorChar));
                    File.Exists(persistedFilePath).ShouldBeTrue();
                    Directory.GetFiles(rawRootPath, "*.tmp", SearchOption.AllDirectories).Any().ShouldBeFalse();
                }
            }
            finally
            {
                if (Directory.Exists(rawRootPath))
                {
                    Directory.Delete(rawRootPath, true);
                }
            }
        }

        [Fact]
        public async Task StoreAsync_Should_Reject_Overwriting_An_Existing_Immutable_Target()
        {
            var rawRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "LocalStorage", Path.GetRandomFileName());
            Directory.CreateDirectory(rawRootPath);

            try
            {
                var storage = new LocalFileSystemRawDatasetStorage(new DatasetStorageOptions
                {
                    RawRootPath = rawRootPath
                });

                using (var firstContent = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
                {
                    await storage.StoreAsync(new StoreRawDatasetFileRequest
                    {
                        TenantId = 4,
                        DatasetId = 8,
                        DatasetVersionId = 15,
                        OriginalFileName = "customers.csv",
                        Content = firstContent
                    });
                }

                using (var duplicateContent = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
                {
                    await Should.ThrowAsync<IOException>(() =>
                        storage.StoreAsync(new StoreRawDatasetFileRequest
                        {
                            TenantId = 4,
                            DatasetId = 8,
                            DatasetVersionId = 15,
                            OriginalFileName = "customers.csv",
                            Content = duplicateContent
                        }));
                }

                Directory.GetFiles(rawRootPath, "*.tmp", SearchOption.AllDirectories).Any().ShouldBeFalse();
            }
            finally
            {
                if (Directory.Exists(rawRootPath))
                {
                    Directory.Delete(rawRootPath, true);
                }
            }
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_A_Previously_Stored_File_And_Tolerate_Missing_Targets()
        {
            var rawRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "LocalStorage", Path.GetRandomFileName());
            Directory.CreateDirectory(rawRootPath);

            try
            {
                var storage = new LocalFileSystemRawDatasetStorage(new DatasetStorageOptions
                {
                    RawRootPath = rawRootPath
                });

                StoredRawDatasetFileResult storedFile;
                using (var content = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
                {
                    storedFile = await storage.StoreAsync(new StoreRawDatasetFileRequest
                    {
                        TenantId = 4,
                        DatasetId = 8,
                        DatasetVersionId = 15,
                        OriginalFileName = "customers.csv",
                        Content = content
                    });
                }

                var persistedFilePath = Path.Combine(rawRootPath, storedFile.StorageKey.Replace('/', Path.DirectorySeparatorChar));
                File.Exists(persistedFilePath).ShouldBeTrue();

                await storage.DeleteAsync(new DeleteRawDatasetFileRequest
                {
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey
                });

                File.Exists(persistedFilePath).ShouldBeFalse();

                await storage.DeleteAsync(new DeleteRawDatasetFileRequest
                {
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey
                });
            }
            finally
            {
                if (Directory.Exists(rawRootPath))
                {
                    Directory.Delete(rawRootPath, true);
                }
            }
        }

        [Fact]
        public async Task OpenReadAsync_Should_Return_The_Previously_Stored_File_Content()
        {
            var rawRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "LocalStorage", Path.GetRandomFileName());
            Directory.CreateDirectory(rawRootPath);

            try
            {
                var storage = new LocalFileSystemRawDatasetStorage(new DatasetStorageOptions
                {
                    RawRootPath = rawRootPath
                });

                StoredRawDatasetFileResult storedFile;
                using (var content = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Alice\n")))
                {
                    storedFile = await storage.StoreAsync(new StoreRawDatasetFileRequest
                    {
                        TenantId = 4,
                        DatasetId = 8,
                        DatasetVersionId = 15,
                        OriginalFileName = "customers.csv",
                        Content = content
                    });
                }

                using (var contentStream = await storage.OpenReadAsync(new OpenReadRawDatasetFileRequest
                {
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey
                }))
                using (var reader = new StreamReader(contentStream, Encoding.UTF8))
                {
                    var fileContent = await reader.ReadToEndAsync();
                    fileContent.ShouldBe("id,name\n1,Alice\n");
                }
            }
            finally
            {
                if (Directory.Exists(rawRootPath))
                {
                    Directory.Delete(rawRootPath, true);
                }
            }
        }
    }
}
