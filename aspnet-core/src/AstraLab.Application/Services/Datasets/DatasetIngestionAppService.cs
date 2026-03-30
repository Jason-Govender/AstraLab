using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Ingestion;
using AstraLab.Services.Datasets.Storage;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides ingestion workflows for validated raw dataset uploads.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetIngestionAppService : AstraLabAppServiceBase, IDatasetIngestionAppService
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IRawDatasetUploadValidator _rawDatasetUploadValidator;
        private readonly IRawDatasetMetadataExtractor _rawDatasetMetadataExtractor;
        private readonly IDatasetRawFileManager _datasetRawFileManager;
        private readonly IDatasetProfilingManager _datasetProfilingManager;
        private readonly IRawDatasetStorage _rawDatasetStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetIngestionAppService"/> class.
        /// </summary>
        public DatasetIngestionAppService(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetFile, long> datasetFileRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IRawDatasetUploadValidator rawDatasetUploadValidator,
            IRawDatasetMetadataExtractor rawDatasetMetadataExtractor,
            IDatasetRawFileManager datasetRawFileManager,
            IDatasetProfilingManager datasetProfilingManager,
            IRawDatasetStorage rawDatasetStorage)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetFileRepository = datasetFileRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _rawDatasetUploadValidator = rawDatasetUploadValidator;
            _rawDatasetMetadataExtractor = rawDatasetMetadataExtractor;
            _datasetRawFileManager = datasetRawFileManager;
            _datasetProfilingManager = datasetProfilingManager;
            _rawDatasetStorage = rawDatasetStorage;
        }

        /// <summary>
        /// Uploads a raw dataset file after validating its format and content.
        /// </summary>
        public async Task<UploadedRawDatasetDto> UploadRawAsync(UploadRawDatasetRequest input)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin())
            {
                var tenantId = GetRequiredTenantId();
                var ownerUserId = AbpSession.GetUserId();
                var datasetFormat = await _rawDatasetUploadValidator.ValidateAsync(input);
                var extractedMetadata = _rawDatasetMetadataExtractor.Extract(input.Content, datasetFormat);
                Dataset dataset = null;
                DatasetVersion datasetVersion = null;
                StoredRawDatasetFileResult storedRawFile = null;

                try
                {
                    dataset = await _datasetRepository.InsertAsync(new Dataset
                    {
                        TenantId = tenantId,
                        Name = input.Name.Trim(),
                        Description = input.Description?.Trim(),
                        SourceFormat = datasetFormat,
                        Status = DatasetStatus.Uploaded,
                        OwnerUserId = ownerUserId,
                        OriginalFileName = Path.GetFileName(input.OriginalFileName.Trim())
                    });

                    await CurrentUnitOfWork.SaveChangesAsync();

                    datasetVersion = await _datasetVersionRepository.InsertAsync(new DatasetVersion
                    {
                        TenantId = tenantId,
                        DatasetId = dataset.Id,
                        VersionNumber = 1,
                        VersionType = DatasetVersionType.Raw,
                        Status = DatasetVersionStatus.Active,
                        SizeBytes = input.Content.LongLength,
                        ColumnCount = extractedMetadata.ColumnCount,
                        SchemaJson = extractedMetadata.SchemaJson
                    });

                    await CurrentUnitOfWork.SaveChangesAsync();

                    dataset.CurrentVersionId = datasetVersion.Id;
                    await CurrentUnitOfWork.SaveChangesAsync();

                    using (var contentStream = new MemoryStream(input.Content, writable: false))
                    {
                        storedRawFile = await _datasetRawFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                        {
                            DatasetId = dataset.Id,
                            DatasetVersionId = datasetVersion.Id,
                            OriginalFileName = input.OriginalFileName,
                            ContentType = input.ContentType,
                            Content = contentStream
                        });
                    }

                    await PersistExtractedColumnsAsync(datasetVersion.Id, tenantId, extractedMetadata.Columns);
                    var datasetProfile = await _datasetProfilingManager.ProfileAsync(datasetVersion.Id);

                    dataset = await _datasetRepository.GetAll()
                        .FirstAsync(item => item.TenantId == tenantId && item.Id == dataset.Id);

                    var persistedColumns = await _datasetColumnRepository.GetAll()
                        .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                        .OrderBy(item => item.Ordinal)
                        .ToListAsync();

                    var output = new UploadedRawDatasetDto
                    {
                        Dataset = ObjectMapper.Map<DatasetDto>(dataset),
                        DatasetVersionId = datasetVersion.Id,
                        StorageProvider = storedRawFile.StorageProvider,
                        StorageKey = storedRawFile.StorageKey,
                        SizeBytes = storedRawFile.SizeBytes,
                        ChecksumSha256 = storedRawFile.ChecksumSha256,
                        ColumnCount = extractedMetadata.ColumnCount,
                        SchemaJson = extractedMetadata.SchemaJson,
                        Columns = ObjectMapper.Map<List<DatasetColumnDto>>(persistedColumns),
                        RowCount = datasetProfile.RowCount,
                        DuplicateRowCount = datasetProfile.DuplicateRowCount,
                        DataHealthScore = datasetProfile.DataHealthScore,
                        ColumnProfiles = datasetProfile.ColumnProfiles
                    };

                    await unitOfWork.CompleteAsync();
                    return output;
                }
                catch (System.Exception exception)
                {
                    if (dataset != null)
                    {
                        try
                        {
                            await CleanupFailedIngestionAsync(tenantId, dataset, storedRawFile);
                            await unitOfWork.CompleteAsync();
                        }
                        catch (System.Exception cleanupException)
                        {
                            throw new System.AggregateException("Dataset ingestion failed and cleanup also failed.", exception, cleanupException);
                        }
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Persists the extracted dataset columns for the uploaded dataset version.
        /// </summary>
        private async Task PersistExtractedColumnsAsync(
            long datasetVersionId,
            int tenantId,
            IReadOnlyList<ExtractedDatasetColumn> extractedColumns)
        {
            if (extractedColumns == null || extractedColumns.Count == 0)
            {
                return;
            }

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

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset upload operations.");
            }

            return AbpSession.TenantId.Value;
        }

        /// <summary>
        /// Deletes a stored raw file using the logical storage reference returned by the storage manager.
        /// </summary>
        private Task DeleteStoredRawFileAsync(StoredRawDatasetFileResult storedRawFile)
        {
            return _rawDatasetStorage.DeleteAsync(new DeleteRawDatasetFileRequest
            {
                StorageProvider = storedRawFile.StorageProvider,
                StorageKey = storedRawFile.StorageKey
            });
        }

        /// <summary>
        /// Cleans up a failed ingestion by deleting the raw file and removing the partially created dataset aggregate.
        /// </summary>
        private async Task CleanupFailedIngestionAsync(int tenantId, Dataset dataset, StoredRawDatasetFileResult storedRawFile)
        {
            if (storedRawFile != null)
            {
                await DeleteStoredRawFileAsync(storedRawFile);
                await CleanupCommittedRawFileRecordAsync(storedRawFile);
            }

            await DeleteDatasetAggregateRecordsAsync(tenantId, dataset);

            using (var unitOfWork = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                await DeleteDatasetAggregateRecordsAsync(tenantId, dataset.Id);
                await unitOfWork.CompleteAsync();
            }
        }

        /// <summary>
        /// Deletes the dataset aggregate records that may have been partially persisted during a failed ingestion.
        /// </summary>
        private async Task DeleteDatasetAggregateRecordsAsync(int tenantId, Dataset dataset)
        {
            if (dataset == null)
            {
                return;
            }

            dataset.CurrentVersionId = null;
            await CurrentUnitOfWork.SaveChangesAsync();

            var datasetVersionIds = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == dataset.Id)
                .Select(item => item.Id)
                .ToListAsync();

            var datasetProfiles = await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && datasetVersionIds.Contains(item.DatasetVersionId))
                .ToListAsync();

            var datasetProfileIds = datasetProfiles.Select(item => item.Id).ToList();

            var datasetColumnProfiles = await _datasetColumnProfileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && datasetProfileIds.Contains(item.DatasetProfileId))
                .ToListAsync();

            foreach (var datasetColumnProfile in datasetColumnProfiles)
            {
                await _datasetColumnProfileRepository.DeleteAsync(datasetColumnProfile);
            }

            foreach (var datasetProfile in datasetProfiles)
            {
                await _datasetProfileRepository.DeleteAsync(datasetProfile);
            }

            var datasetColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && datasetVersionIds.Contains(item.DatasetVersionId))
                .ToListAsync();

            foreach (var datasetColumn in datasetColumns)
            {
                await _datasetColumnRepository.DeleteAsync(datasetColumn);
            }

            var datasetFiles = await _datasetFileRepository.GetAll()
                .Where(item => item.TenantId == tenantId && datasetVersionIds.Contains(item.DatasetVersionId))
                .ToListAsync();

            foreach (var datasetFile in datasetFiles)
            {
                await _datasetFileRepository.DeleteAsync(datasetFile);
            }

            var datasetVersions = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == dataset.Id)
                .ToListAsync();

            foreach (var datasetVersion in datasetVersions)
            {
                await _datasetVersionRepository.DeleteAsync(datasetVersion);
            }

            await _datasetRepository.DeleteAsync(dataset);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes dataset aggregate records in an isolated cleanup unit of work.
        /// </summary>
        private async Task DeleteDatasetAggregateRecordsAsync(int tenantId, long datasetId)
        {
            var dataset = await _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Id == datasetId)
                .FirstOrDefaultAsync();

            if (dataset == null)
            {
                return;
            }

            await DeleteDatasetAggregateRecordsAsync(tenantId, dataset);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Removes any raw file record that was already committed by the raw file manager.
        /// </summary>
        private async Task CleanupCommittedRawFileRecordAsync(StoredRawDatasetFileResult storedRawFile)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                var datasetFiles = await _datasetFileRepository.GetAll()
                    .Where(item =>
                        item.StorageProvider == storedRawFile.StorageProvider &&
                        item.StorageKey == storedRawFile.StorageKey)
                    .ToListAsync();

                foreach (var datasetFile in datasetFiles)
                {
                    await _datasetFileRepository.DeleteAsync(datasetFile);
                }

                await unitOfWork.CompleteAsync();
            }
        }
    }
}
