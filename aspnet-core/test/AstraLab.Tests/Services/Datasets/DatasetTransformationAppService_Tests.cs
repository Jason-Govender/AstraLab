using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.MicroKernel.Registration;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Web.Core.Datasets.Storage;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetTransformationAppService_Tests : AstraLabTestBase
    {
        private static readonly JsonSerializerOptions ConfigurationSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        private readonly IDatasetIngestionAppService _datasetIngestionAppService;
        private readonly IDatasetTransformationAppService _datasetTransformationAppService;
        private readonly IRawDatasetStorage _rawDatasetStorage;

        public DatasetTransformationAppService_Tests()
        {
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
            _datasetTransformationAppService = Resolve<IDatasetTransformationAppService>();
            _rawDatasetStorage = Resolve<IRawDatasetStorage>();
        }

        [Fact]
        public async Task TransformAsync_Should_Create_Processed_Version_Transformation_File_And_Profile_For_RemoveDuplicates()
        {
            var upload = await UploadCsvAsync(
                "Remove duplicates",
                "id,name\n1,Alice\n1,Alice\n2,Bob\n");

            var result = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.RemoveDuplicates,
                        ConfigurationJson = SerializeConfiguration(new { })
                    }
                }
            });

            result.SourceDatasetVersionId.ShouldBe(upload.DatasetVersionId);
            result.CreatedVersions.Count.ShouldBe(1);
            result.Transformations.Count.ShouldBe(1);
            result.FinalDatasetVersionId.ShouldBe(result.CreatedVersions.Single().Id);
            result.FinalProfile.RowCount.ShouldBe(2);
            result.FinalProfile.DuplicateRowCount.ShouldBe(0);
            result.Transformations.Single().TransformationType.ShouldBe(DatasetTransformationType.RemoveDuplicates);
            result.Transformations.Single().SourceDatasetVersionId.ShouldBe(upload.DatasetVersionId);
            result.Transformations.Single().ResultDatasetVersionId.ShouldBe(result.FinalDatasetVersionId);
            result.Transformations.Single().ConfigurationJson.ShouldContain("\"columns\":[]");

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                var createdVersion = context.DatasetVersions.Single(item => item.Id == result.FinalDatasetVersionId);
                var createdFile = context.DatasetFiles.Single(item => item.DatasetVersionId == createdVersion.Id);
                var createdColumns = context.DatasetColumns
                    .Where(item => item.DatasetVersionId == createdVersion.Id)
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                dataset.CurrentVersionId.ShouldBe(createdVersion.Id);
                dataset.Status.ShouldBe(DatasetStatus.Ready);
                createdVersion.VersionType.ShouldBe(DatasetVersionType.Processed);
                createdVersion.ParentVersionId.ShouldBe(upload.DatasetVersionId);
                createdVersion.Status.ShouldBe(DatasetVersionStatus.Active);
                createdVersion.RowCount.ShouldBe(2);
                createdFile.StorageKey.ShouldContain("/processed/");
                createdColumns.Select(item => item.Name).ShouldBe(new[] { "id", "name" });
                context.DatasetTransformations.Count().ShouldBe(1);
                context.DatasetProfiles.Count().ShouldBe(2);
                context.DatasetColumnProfiles.Count(item => item.DatasetProfile.DatasetVersionId == createdVersion.Id).ShouldBe(2);
                await Task.CompletedTask;
            });

            var content = await ReadVersionContentAsync(result.FinalDatasetVersionId);
            content.ShouldContain("1,Alice");
            content.ShouldContain("2,Bob");
            content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length.ShouldBe(3);
        }

        [Fact]
        public async Task TransformAsync_Should_Create_Chained_Processed_Versions_For_MultiStep_Pipeline()
        {
            var upload = await UploadCsvAsync(
                "Pipeline CSV",
                "id,name\n1,Alice\n1,Alice\n2,Bob\n3,Carla\n");

            var result = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.RemoveDuplicates,
                        ConfigurationJson = SerializeConfiguration(new { })
                    },
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.FilterRows,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            match = "all",
                            conditions = new[]
                            {
                                new
                                {
                                    column = "id",
                                    @operator = "greaterThan",
                                    value = "1"
                                }
                            }
                        })
                    }
                }
            });

            result.CreatedVersions.Count.ShouldBe(2);
            result.Transformations.Count.ShouldBe(2);
            result.CreatedVersions[0].ParentVersionId.ShouldBe(upload.DatasetVersionId);
            result.CreatedVersions[1].ParentVersionId.ShouldBe(result.CreatedVersions[0].Id);
            result.CreatedVersions[0].Status.ShouldBe(DatasetVersionStatus.Superseded);
            result.CreatedVersions[1].Status.ShouldBe(DatasetVersionStatus.Active);
            result.FinalDatasetVersionId.ShouldBe(result.CreatedVersions[1].Id);
            result.Transformations[0].SourceDatasetVersionId.ShouldBe(upload.DatasetVersionId);
            result.Transformations[0].ResultDatasetVersionId.ShouldBe(result.CreatedVersions[0].Id);
            result.Transformations[1].SourceDatasetVersionId.ShouldBe(result.CreatedVersions[0].Id);
            result.Transformations[1].ResultDatasetVersionId.ShouldBe(result.CreatedVersions[1].Id);
            result.FinalProfile.RowCount.ShouldBe(2);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.CurrentVersionId.ShouldBe(result.FinalDatasetVersionId);
                context.DatasetVersions.Count(item => item.DatasetId == upload.Dataset.Id).ShouldBe(3);
                context.DatasetTransformations.Count().ShouldBe(2);
                await Task.CompletedTask;
            });

            var content = await ReadVersionContentAsync(result.FinalDatasetVersionId);
            content.ShouldContain("2,Bob");
            content.ShouldContain("3,Carla");
            content.ShouldNotContain("1,Alice");
        }

        [Fact]
        public async Task TransformAsync_Should_Allow_Transforming_An_Already_Processed_Version()
        {
            var upload = await UploadCsvAsync(
                "Processed source CSV",
                "id,name\n1,Alice\n2,Bob\n3,Carla\n");

            var firstTransformation = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.FilterRows,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            match = "all",
                            conditions = new[]
                            {
                                new
                                {
                                    column = "id",
                                    @operator = "greaterThan",
                                    value = "1"
                                }
                            }
                        })
                    }
                }
            });

            var secondTransformation = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = firstTransformation.FinalDatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.FilterRows,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            match = "all",
                            conditions = new[]
                            {
                                new
                                {
                                    column = "name",
                                    @operator = "contains",
                                    value = "arl"
                                }
                            }
                        })
                    }
                }
            });

            secondTransformation.CreatedVersions.Count.ShouldBe(1);
            secondTransformation.CreatedVersions.Single().ParentVersionId.ShouldBe(firstTransformation.FinalDatasetVersionId);
            secondTransformation.FinalProfile.RowCount.ShouldBe(1);

            await UsingDbContextAsync(async context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.CurrentVersionId.ShouldBe(secondTransformation.FinalDatasetVersionId);
                context.DatasetVersions.Count(item => item.DatasetId == upload.Dataset.Id).ShouldBe(3);
                context.DatasetTransformations.Count().ShouldBe(2);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task TransformAsync_Should_Handle_Missing_Values_Using_FillMean()
        {
            var upload = await UploadCsvAsync(
                "Missing values CSV",
                "id,amount\n1,10\n2,\n3,25\n");

            var result = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.HandleMissingValues,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            columns = new[] { "amount" },
                            strategy = "fillMean"
                        })
                    }
                }
            });

            result.FinalProfile.RowCount.ShouldBe(3);
            result.FinalProfile.TotalNullCount.ShouldBe(0);

            await UsingDbContextAsync(async context =>
            {
                var amountColumn = context.DatasetColumns.Single(item =>
                    item.DatasetVersionId == result.FinalDatasetVersionId &&
                    item.Name == "amount");

                amountColumn.DataType.ShouldBe("decimal");
                amountColumn.NullCount.ShouldBe(0);
                await Task.CompletedTask;
            });

            var content = await ReadVersionContentAsync(result.FinalDatasetVersionId);
            content.ShouldContain("2,17.5");
        }

        [Fact]
        public async Task TransformAsync_Should_Preserve_Explicit_Converted_Data_Types()
        {
            var upload = await UploadCsvAsync(
                "Convert CSV",
                "id,createdAt\n1,2024-01-01T00:00:00Z\n2,2024-01-02T00:00:00Z\n");

            var result = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.ConvertDataType,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            column = "createdAt",
                            targetType = "string"
                        })
                    }
                }
            });

            await UsingDbContextAsync(async context =>
            {
                var convertedColumn = context.DatasetColumns.Single(item =>
                    item.DatasetVersionId == result.FinalDatasetVersionId &&
                    item.Name == "createdAt");

                convertedColumn.DataType.ShouldBe("string");
                convertedColumn.IsDataTypeInferred.ShouldBeFalse();

                var columnProfile = context.DatasetColumnProfiles.Single(item => item.DatasetColumnId == convertedColumn.Id);
                columnProfile.InferredDataType.ShouldBe("string");
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task TransformAsync_Should_Reject_Invalid_Data_Type_Conversions_Clearly()
        {
            var upload = await UploadCsvAsync(
                "Invalid convert CSV",
                "name\nAlice\nBob\n");

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = upload.DatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.ConvertDataType,
                            ConfigurationJson = SerializeConfiguration(new
                            {
                                column = "name",
                                targetType = "integer"
                            })
                        }
                    }
                }));

            exception.Message.ShouldBe("Column 'name' contains a value that cannot be converted to integer.");
        }

        [Fact]
        public async Task TransformAsync_Should_Aggregate_Tabular_Json_And_Preserve_Json_Format()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Aggregate JSON",
                OriginalFileName = "aggregate.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("[{\"category\":\"A\",\"amount\":10},{\"category\":\"A\",\"amount\":5},{\"category\":\"B\",\"amount\":2}]")
            });

            var result = await _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
            {
                SourceDatasetVersionId = upload.DatasetVersionId,
                Steps = new List<DatasetTransformationStepRequest>
                {
                    new DatasetTransformationStepRequest
                    {
                        TransformationType = DatasetTransformationType.Aggregate,
                        ConfigurationJson = SerializeConfiguration(new
                        {
                            groupByColumns = new[] { "category" },
                            aggregations = new object[]
                            {
                                new
                                {
                                    function = "sum",
                                    column = "amount",
                                    outputColumn = "totalAmount"
                                },
                                new
                                {
                                    function = "count",
                                    outputColumn = "rowCount"
                                }
                            }
                        })
                    }
                }
            });

            result.FinalProfile.RowCount.ShouldBe(2);

            await UsingDbContextAsync(async context =>
            {
                var columns = context.DatasetColumns
                    .Where(item => item.DatasetVersionId == result.FinalDatasetVersionId)
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                columns.Select(item => item.Name).ShouldBe(new[] { "category", "totalAmount", "rowCount" });
                await Task.CompletedTask;
            });

            var content = await ReadVersionContentAsync(result.FinalDatasetVersionId);
            content.ShouldStartWith("[");
            content.ShouldContain("\"category\":\"A\"");
            content.ShouldContain("\"totalAmount\":15");
            content.ShouldContain("\"rowCount\":2");
        }

        [Fact]
        public async Task TransformAsync_Should_Reject_NonTabular_Json_Cleanly()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Scalar JSON",
                OriginalFileName = "scalar.json",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes("\"hello\"")
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = upload.DatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.RemoveDuplicates,
                            ConfigurationJson = SerializeConfiguration(new { })
                        }
                    }
                }));

            exception.Message.ShouldBe("Only tabular dataset versions with persisted columns can be transformed.");
        }

        [Fact]
        public async Task TransformAsync_Should_Delete_Partial_Artifacts_When_A_Later_Profile_Fails()
        {
            var upload = await UploadCsvAsync(
                "Cleanup CSV",
                "id,name\n1,Alice\n1,Alice\n2,Bob\n");
            var datasetStorageOptions = Resolve<DatasetStorageOptions>();
            var fileCountBefore = CountStoredFiles(datasetStorageOptions.RawRootPath);

            LocalIocManager.IocContainer.Register(
                Component.For<IDatasetProfilingManager>()
                    .Instance(new FailOnSecondProfileDatasetProfilingManager(Resolve<IDatasetProfilingManager>()))
                    .IsDefault()
                    .LifestyleSingleton());

            var transformationAppService = Resolve<IDatasetTransformationAppService>();

            await Should.ThrowAsync<IOException>(() =>
                transformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = upload.DatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.RemoveDuplicates,
                            ConfigurationJson = SerializeConfiguration(new { })
                        },
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.FilterRows,
                            ConfigurationJson = SerializeConfiguration(new
                            {
                                match = "all",
                                conditions = new[]
                                {
                                    new
                                    {
                                        column = "id",
                                        @operator = "greaterThan",
                                        value = "1"
                                    }
                                }
                            })
                        }
                    }
                }));

            await UsingDbContextAsync(async context =>
            {
                context.Datasets.Count(item => !item.IsDeleted).ShouldBe(1);
                context.DatasetVersions.Count(item => !item.IsDeleted).ShouldBe(1);
                context.DatasetFiles.Count(item => !item.IsDeleted).ShouldBe(1);
                context.DatasetColumns.Count(item => !item.IsDeleted).ShouldBe(2);
                context.DatasetProfiles.Count(item => !item.IsDeleted).ShouldBe(1);
                context.DatasetColumnProfiles.Count(item => !item.IsDeleted).ShouldBe(2);
                context.DatasetTransformations.Count(item => !item.IsDeleted).ShouldBe(0);
                await Task.CompletedTask;
            });

            CountStoredFiles(datasetStorageOptions.RawRootPath).ShouldBe(fileCountBefore);
        }

        [Fact]
        public async Task TransformAsync_Should_Reject_Cross_Owner_And_Host_Context()
        {
            var upload = await UploadCsvAsync(
                "Owner CSV",
                "id,name\n1,Alice\n");

            UsingDbContext(context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.OwnerUserId = dataset.OwnerUserId + 500;
                context.SaveChanges();
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = upload.DatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.RemoveDuplicates,
                            ConfigurationJson = SerializeConfiguration(new { })
                        }
                    }
                }));

            LoginAsHostAdmin();

            var hostException = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = upload.DatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.RemoveDuplicates,
                            ConfigurationJson = SerializeConfiguration(new { })
                        }
                    }
                }));

            hostException.Message.ShouldBe("Tenant context is required for dataset transformation operations.");
        }

        [Fact]
        public async Task TransformAsync_Should_Reject_Cross_Tenant_Dataset_Versions()
        {
            var otherTenantId = UsingDbContext((int?)null, context =>
            {
                var tenant = context.Tenants.Add(new Tenant("transform-other", "Transform Other Tenant")).Entity;
                context.SaveChanges();
                return tenant.Id;
            });

            var otherTenantDatasetVersionId = UsingDbContext(otherTenantId, context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = otherTenantId,
                    Name = "other-tenant-dataset",
                    Description = "other tenant",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
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
                    SizeBytes = 32,
                    ColumnCount = 1
                }).Entity;

                context.SaveChanges();
                return datasetVersion.Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetTransformationAppService.TransformAsync(new TransformDatasetVersionRequest
                {
                    SourceDatasetVersionId = otherTenantDatasetVersionId,
                    Steps = new List<DatasetTransformationStepRequest>
                    {
                        new DatasetTransformationStepRequest
                        {
                            TransformationType = DatasetTransformationType.RemoveDuplicates,
                            ConfigurationJson = SerializeConfiguration(new { })
                        }
                    }
                }));
        }

        private async Task<UploadedRawDatasetDto> UploadCsvAsync(string name, string content)
        {
            return await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = name,
                OriginalFileName = $"{name.Replace(" ", "-", StringComparison.OrdinalIgnoreCase)}.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes(content)
            });
        }

        private async Task<string> ReadVersionContentAsync(long datasetVersionId)
        {
            var storageReference = await UsingDbContextAsync(async context =>
            {
                var datasetFile = context.DatasetFiles.Single(item => item.DatasetVersionId == datasetVersionId);
                return new
                {
                    datasetFile.StorageProvider,
                    datasetFile.StorageKey
                };
            });

            using (var stream = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
            {
                StorageProvider = storageReference.StorageProvider,
                StorageKey = storageReference.StorageKey
            }))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static string SerializeConfiguration(object value)
        {
            return JsonSerializer.Serialize(value, ConfigurationSerializerOptions);
        }

        private static int CountStoredFiles(string rootPath)
        {
            return Directory.Exists(rootPath)
                ? Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories).Length
                : 0;
        }

        private class FailOnSecondProfileDatasetProfilingManager : IDatasetProfilingManager
        {
            private readonly IDatasetProfilingManager _inner;
            private int _attemptCount;

            public FailOnSecondProfileDatasetProfilingManager(IDatasetProfilingManager inner)
            {
                _inner = inner;
            }

            public async Task<DatasetProfileDto> ProfileAsync(long datasetVersionId)
            {
                _attemptCount++;
                if (_attemptCount >= 2)
                {
                    throw new IOException("Simulated transformation profiling failure.");
                }

                return await _inner.ProfileAsync(datasetVersionId);
            }
        }
    }
}
