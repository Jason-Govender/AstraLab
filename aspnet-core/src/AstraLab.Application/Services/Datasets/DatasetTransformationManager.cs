using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Ingestion;
using AstraLab.Services.Datasets.Profiling;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.Datasets.Transformations;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Orchestrates dataset transformation pipelines and persists processed versions.
    /// </summary>
    public class DatasetTransformationManager : AstraLabAppServiceBase, IDatasetTransformationManager, ITransientDependency
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IRepository<DatasetTransformation, long> _datasetTransformationRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IDatasetVersionFileManager _datasetVersionFileManager;
        private readonly IDatasetTransformationPipelineExecutor _datasetTransformationPipelineExecutor;
        private readonly IDatasetTabularDataCodec _datasetTabularDataCodec;
        private readonly IDatasetProfilingManager _datasetProfilingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransformationManager"/> class.
        /// </summary>
        public DatasetTransformationManager(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetFile, long> datasetFileRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IRepository<DatasetTransformation, long> datasetTransformationRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IRawDatasetStorage rawDatasetStorage,
            IDatasetVersionFileManager datasetVersionFileManager,
            IDatasetTransformationPipelineExecutor datasetTransformationPipelineExecutor,
            IDatasetTabularDataCodec datasetTabularDataCodec,
            IDatasetProfilingManager datasetProfilingManager)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetFileRepository = datasetFileRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _datasetTransformationRepository = datasetTransformationRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _rawDatasetStorage = rawDatasetStorage;
            _datasetVersionFileManager = datasetVersionFileManager;
            _datasetTransformationPipelineExecutor = datasetTransformationPipelineExecutor;
            _datasetTabularDataCodec = datasetTabularDataCodec;
            _datasetProfilingManager = datasetProfilingManager;
        }

        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        public async Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input)
        {
            ValidateRequest(input);

            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var sourceDatasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.SourceDatasetVersionId, tenantId, ownerUserId);
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(sourceDatasetVersion.DatasetId, tenantId, ownerUserId);

            if (sourceDatasetVersion.RawFile == null)
            {
                throw new UserFriendlyException("The dataset version does not have a stored dataset file to transform.");
            }

            var sourceColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == sourceDatasetVersion.Id)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            if (sourceColumns.Count == 0)
            {
                throw new UserFriendlyException("Only tabular dataset versions with persisted columns can be transformed.");
            }

            TabularDataset sourceDataset;
            using (var sourceContent = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
            {
                StorageProvider = sourceDatasetVersion.RawFile.StorageProvider,
                StorageKey = sourceDatasetVersion.RawFile.StorageKey
            }))
            {
                sourceDataset = await _datasetTabularDataCodec.ReadAsync(dataset.SourceFormat, sourceColumns, sourceContent);
            }

            var pipelineResults = _datasetTransformationPipelineExecutor.Execute(sourceDataset, input.Steps);
            var createdVersions = new List<DatasetVersion>();
            var createdTransformations = new List<DatasetTransformation>();
            var storedFiles = new List<StoredRawDatasetFileResult>();
            DatasetProfileSummaryDto finalProfile = null;
            DatasetVersion currentSourceVersion = sourceDatasetVersion;
            var nextVersionNumber = await GetNextVersionNumberAsync(dataset.Id, tenantId);
            var originalDatasetStatus = dataset.Status;
            var originalCurrentVersionId = dataset.CurrentVersionId;

            try
            {
                for (var index = 0; index < pipelineResults.Count; index++)
                {
                    var stepResult = pipelineResults[index];
                    var isFinalStep = index == pipelineResults.Count - 1;
                    var transformedMetadata = _datasetTabularDataCodec.BuildMetadata(stepResult.Dataset, dataset.SourceFormat);
                    var transformedContent = _datasetTabularDataCodec.Write(stepResult.Dataset, dataset.SourceFormat);

                    var createdVersion = await _datasetVersionRepository.InsertAsync(new DatasetVersion
                    {
                        TenantId = tenantId,
                        DatasetId = dataset.Id,
                        VersionNumber = nextVersionNumber++,
                        VersionType = DatasetVersionType.Processed,
                        Status = DatasetVersionStatus.Draft,
                        ParentVersionId = currentSourceVersion.Id,
                        SizeBytes = transformedContent.LongLength,
                        ColumnCount = transformedMetadata.ColumnCount,
                        SchemaJson = transformedMetadata.SchemaJson
                    });

                    await CurrentUnitOfWork.SaveChangesAsync();
                    createdVersions.Add(createdVersion);

                    using (var transformedStream = new MemoryStream(transformedContent, writable: false))
                    {
                        var storedFile = await _datasetVersionFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                        {
                            DatasetId = dataset.Id,
                            DatasetVersionId = createdVersion.Id,
                            OriginalFileName = BuildProcessedFileName(dataset.OriginalFileName, createdVersion.VersionNumber, dataset.SourceFormat),
                            ContentType = _datasetTabularDataCodec.GetContentType(dataset.SourceFormat),
                            Content = transformedStream,
                            FileKind = DatasetVersionFileKind.Processed
                        });

                        storedFiles.Add(storedFile);
                    }

                    await PersistExtractedColumnsAsync(createdVersion.Id, tenantId, transformedMetadata.Columns);

                    var transformation = await _datasetTransformationRepository.InsertAsync(new DatasetTransformation
                    {
                        TenantId = tenantId,
                        SourceDatasetVersionId = currentSourceVersion.Id,
                        ResultDatasetVersionId = createdVersion.Id,
                        TransformationType = stepResult.TransformationType,
                        ConfigurationJson = stepResult.CanonicalConfigurationJson,
                        ExecutionOrder = await GetNextExecutionOrderAsync(currentSourceVersion.Id, tenantId),
                        ExecutedAt = DateTime.UtcNow,
                        SummaryJson = stepResult.SummaryJson
                    });

                    await CurrentUnitOfWork.SaveChangesAsync();
                    createdTransformations.Add(transformation);

                    var profile = await _datasetProfilingManager.ProfileAsync(createdVersion.Id);
                    finalProfile = BuildProfileSummary(profile);

                    createdVersion.Status = isFinalStep ? DatasetVersionStatus.Active : DatasetVersionStatus.Superseded;
                    if (isFinalStep)
                    {
                        dataset.CurrentVersionId = createdVersion.Id;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    currentSourceVersion = createdVersion;
                }

                return new TransformDatasetVersionResultDto
                {
                    SourceDatasetVersionId = sourceDatasetVersion.Id,
                    FinalDatasetVersionId = createdVersions.Last().Id,
                    CreatedVersions = ObjectMapper.Map<List<DatasetVersionDto>>(createdVersions),
                    Transformations = ObjectMapper.Map<List<DatasetTransformationDto>>(createdTransformations),
                    FinalProfile = finalProfile
                };
            }
            catch (Exception exception)
            {
                Exception fileCleanupException = null;

                try
                {
                    await DeleteStoredFilesAsync(storedFiles);
                }
                catch (Exception cleanupException)
                {
                    fileCleanupException = cleanupException;
                }

                try
                {
                    await CleanupPersistedArtifactsAsync(
                        dataset,
                        originalDatasetStatus,
                        originalCurrentVersionId,
                        createdVersions,
                        createdTransformations);
                }
                catch (Exception cleanupException)
                {
                    if (fileCleanupException != null)
                    {
                        throw new AggregateException(
                            "Dataset transformation failed and cleanup also failed.",
                            exception,
                            fileCleanupException,
                            cleanupException);
                    }

                    throw new AggregateException("Dataset transformation failed and cleanup also failed.", exception, cleanupException);
                }

                if (fileCleanupException != null)
                {
                    throw new AggregateException("Dataset transformation failed and cleanup also failed.", exception, fileCleanupException);
                }

                throw;
            }
        }

        private static void ValidateRequest(TransformDatasetVersionRequest input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.SourceDatasetVersionId <= 0)
            {
                throw new UserFriendlyException("A valid source dataset version is required for transformation.");
            }

            if (input.Steps == null || input.Steps.Count == 0)
            {
                throw new UserFriendlyException("At least one transformation step is required.");
            }

            if (input.Steps.Any(item => item == null))
            {
                throw new UserFriendlyException("Transformation steps cannot contain null entries.");
            }

            if (input.Steps.Any(item => string.IsNullOrWhiteSpace(item.ConfigurationJson)))
            {
                throw new UserFriendlyException("Each transformation step must define a configuration payload.");
            }
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset transformation operations.");
            }

            return AbpSession.TenantId.Value;
        }

        private async Task<int> GetNextVersionNumberAsync(long datasetId, int tenantId)
        {
            var existingVersionNumbers = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == datasetId)
                .Select(item => item.VersionNumber)
                .ToListAsync();

            return existingVersionNumbers.Count == 0 ? 1 : existingVersionNumbers.Max() + 1;
        }

        private async Task<int> GetNextExecutionOrderAsync(long sourceDatasetVersionId, int tenantId)
        {
            var existingExecutionOrders = await _datasetTransformationRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.SourceDatasetVersionId == sourceDatasetVersionId)
                .Select(item => item.ExecutionOrder)
                .ToListAsync();

            return existingExecutionOrders.Count == 0 ? 1 : existingExecutionOrders.Max() + 1;
        }

        private async Task PersistExtractedColumnsAsync(
            long datasetVersionId,
            int tenantId,
            IReadOnlyList<ExtractedDatasetColumn> extractedColumns)
        {
            foreach (var extractedColumn in extractedColumns)
            {
                await _datasetColumnRepository.InsertAsync(new DatasetColumn
                {
                    TenantId = tenantId,
                    DatasetVersionId = datasetVersionId,
                    Name = extractedColumn.Name,
                    DataType = extractedColumn.DataType,
                    IsDataTypeInferred = extractedColumn.IsDataTypeInferred,
                    Ordinal = extractedColumn.Ordinal
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private DatasetProfileSummaryDto BuildProfileSummary(DatasetProfileDto profile)
        {
            var summary = DatasetProfileSerialization.ReadSummary(profile.SummaryJson);
            return new DatasetProfileSummaryDto
            {
                DatasetVersionId = profile.DatasetVersionId,
                ProfileId = profile.Id,
                RowCount = profile.RowCount,
                DuplicateRowCount = profile.DuplicateRowCount,
                DataHealthScore = profile.DataHealthScore,
                TotalNullCount = summary.TotalNullCount,
                OverallNullPercentage = summary.OverallNullPercentage,
                TotalAnomalyCount = summary.TotalAnomalyCount,
                OverallAnomalyPercentage = summary.OverallAnomalyPercentage,
                CreationTime = profile.CreationTime
            };
        }

        private string BuildProcessedFileName(string originalFileName, int versionNumber, DatasetFormat datasetFormat)
        {
            var extension = _datasetTabularDataCodec.GetFileExtension(datasetFormat);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            return $"{fileNameWithoutExtension}.v{versionNumber}{extension}";
        }

        private async Task DeleteStoredFilesAsync(IEnumerable<StoredRawDatasetFileResult> storedFiles)
        {
            foreach (var storedFile in storedFiles.Reverse())
            {
                await _rawDatasetStorage.DeleteAsync(new DeleteRawDatasetFileRequest
                {
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey
                });
            }
        }

        private async Task CleanupPersistedArtifactsAsync(
            Dataset dataset,
            DatasetStatus originalDatasetStatus,
            long? originalCurrentVersionId,
            IReadOnlyList<DatasetVersion> createdVersions,
            IReadOnlyList<DatasetTransformation> createdTransformations)
        {
            var createdVersionIds = createdVersions.Select(item => item.Id).ToList();
            var createdTransformationIds = createdTransformations.Select(item => item.Id).ToList();

            if (createdTransformationIds.Count > 0)
            {
                var persistedTransformations = await _datasetTransformationRepository.GetAll()
                    .Where(item => createdTransformationIds.Contains(item.Id))
                    .ToListAsync();

                foreach (var persistedTransformation in persistedTransformations.OrderByDescending(item => item.Id))
                {
                    await _datasetTransformationRepository.HardDeleteAsync(persistedTransformation);
                }
            }

            if (createdVersionIds.Count > 0)
            {
                var persistedProfiles = await _datasetProfileRepository.GetAll()
                    .Include(item => item.ColumnProfiles)
                    .Where(item => createdVersionIds.Contains(item.DatasetVersionId))
                    .ToListAsync();

                foreach (var persistedColumnProfile in persistedProfiles
                    .SelectMany(item => item.ColumnProfiles)
                    .OrderByDescending(item => item.Id))
                {
                    await _datasetColumnProfileRepository.HardDeleteAsync(persistedColumnProfile);
                }

                foreach (var persistedProfile in persistedProfiles.OrderByDescending(item => item.Id))
                {
                    await _datasetProfileRepository.HardDeleteAsync(persistedProfile);
                }

                var persistedColumns = await _datasetColumnRepository.GetAll()
                    .Where(item => createdVersionIds.Contains(item.DatasetVersionId))
                    .ToListAsync();

                foreach (var persistedColumn in persistedColumns.OrderByDescending(item => item.Id))
                {
                    await _datasetColumnRepository.HardDeleteAsync(persistedColumn);
                }

                var persistedFiles = await _datasetFileRepository.GetAll()
                    .Where(item => createdVersionIds.Contains(item.DatasetVersionId))
                    .ToListAsync();

                foreach (var persistedFile in persistedFiles.OrderByDescending(item => item.Id))
                {
                    await _datasetFileRepository.HardDeleteAsync(persistedFile);
                }

                var persistedVersions = await _datasetVersionRepository.GetAll()
                    .Where(item => createdVersionIds.Contains(item.Id))
                    .ToListAsync();

                foreach (var persistedVersion in persistedVersions.OrderByDescending(item => item.Id))
                {
                    await _datasetVersionRepository.HardDeleteAsync(persistedVersion);
                }
            }

            dataset.Status = originalDatasetStatus;
            dataset.CurrentVersionId = originalCurrentVersionId;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
