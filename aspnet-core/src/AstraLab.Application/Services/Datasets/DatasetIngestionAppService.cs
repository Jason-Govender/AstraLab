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
        private readonly IRawDatasetUploadValidator _rawDatasetUploadValidator;
        private readonly IRawDatasetMetadataExtractor _rawDatasetMetadataExtractor;
        private readonly IDatasetRawFileManager _datasetRawFileManager;
        private readonly IRawDatasetStorage _rawDatasetStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetIngestionAppService"/> class.
        /// </summary>
        public DatasetIngestionAppService(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetFile, long> datasetFileRepository,
            IRawDatasetUploadValidator rawDatasetUploadValidator,
            IRawDatasetMetadataExtractor rawDatasetMetadataExtractor,
            IDatasetRawFileManager datasetRawFileManager,
            IRawDatasetStorage rawDatasetStorage)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetFileRepository = datasetFileRepository;
            _rawDatasetUploadValidator = rawDatasetUploadValidator;
            _rawDatasetMetadataExtractor = rawDatasetMetadataExtractor;
            _datasetRawFileManager = datasetRawFileManager;
            _rawDatasetStorage = rawDatasetStorage;
        }

        /// <summary>
        /// Uploads a raw dataset file after validating its format and content.
        /// </summary>
        public async Task<UploadedRawDatasetDto> UploadRawAsync(UploadRawDatasetRequest input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var datasetFormat = await _rawDatasetUploadValidator.ValidateAsync(input);
            var extractedMetadata = _rawDatasetMetadataExtractor.Extract(input.Content, datasetFormat);

            var dataset = await _datasetRepository.InsertAsync(new Dataset
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

            var datasetVersion = await _datasetVersionRepository.InsertAsync(new DatasetVersion
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

            StoredRawDatasetFileResult storedRawFile;
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

            try
            {
                await PersistExtractedColumnsAsync(datasetVersion.Id, tenantId, extractedMetadata.Columns);

                dataset = await _datasetRepository.GetAll()
                    .FirstAsync(item => item.TenantId == tenantId && item.Id == dataset.Id);

                var persistedColumns = await _datasetColumnRepository.GetAll()
                    .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                    .OrderBy(item => item.Ordinal)
                    .ToListAsync();

                return new UploadedRawDatasetDto
                {
                    Dataset = ObjectMapper.Map<DatasetDto>(dataset),
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = storedRawFile.StorageProvider,
                    StorageKey = storedRawFile.StorageKey,
                    SizeBytes = storedRawFile.SizeBytes,
                    ChecksumSha256 = storedRawFile.ChecksumSha256,
                    ColumnCount = extractedMetadata.ColumnCount,
                    SchemaJson = extractedMetadata.SchemaJson,
                    Columns = ObjectMapper.Map<List<DatasetColumnDto>>(persistedColumns)
                };
            }
            catch (System.Exception exception)
            {
                try
                {
                    await CleanupFailedIngestionAsync(tenantId, dataset.Id, storedRawFile);
                }
                catch (System.Exception cleanupException)
                {
                    throw new System.AggregateException("Dataset ingestion failed and cleanup also failed.", exception, cleanupException);
                }

                throw;
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
        private async Task CleanupFailedIngestionAsync(int tenantId, long datasetId, StoredRawDatasetFileResult storedRawFile)
        {
            await DeleteStoredRawFileAsync(storedRawFile);
            await CleanupCommittedRawFileRecordAsync(storedRawFile);

            var dataset = await _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.Id == datasetId)
                .FirstOrDefaultAsync();

            if (dataset == null)
            {
                return;
            }

            dataset.CurrentVersionId = null;
            await CurrentUnitOfWork.SaveChangesAsync();

            var datasetVersionIds = await _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetId == datasetId)
                .Select(item => item.Id)
                .ToListAsync();

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
                .Where(item => item.TenantId == tenantId && item.DatasetId == datasetId)
                .ToListAsync();

            foreach (var datasetVersion in datasetVersions)
            {
                await _datasetVersionRepository.DeleteAsync(datasetVersion);
            }

            await _datasetRepository.DeleteAsync(dataset);
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
