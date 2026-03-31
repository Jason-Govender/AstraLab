using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Persists immutable stored files for tenant-owned dataset versions.
    /// </summary>
    public class DatasetVersionFileManager : IDatasetVersionFileManager, ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IRawDatasetStorage _rawDatasetStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetVersionFileManager"/> class.
        /// </summary>
        public DatasetVersionFileManager(
            IAbpSession abpSession,
            IRepository<DatasetFile, long> datasetFileRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IRawDatasetStorage rawDatasetStorage)
        {
            _abpSession = abpSession;
            _datasetFileRepository = datasetFileRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _rawDatasetStorage = rawDatasetStorage;
        }

        /// <summary>
        /// Stores a dataset file for a tenant-owned dataset version and persists its logical reference.
        /// </summary>
        public async Task<StoredRawDatasetFileResult> StoreForVersionAsync(StoreRawDatasetFileRequest request)
        {
            ValidateRequest(request);

            var tenantId = GetRequiredTenantId();
            var ownerUserId = _abpSession.GetUserId();
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(request.DatasetVersionId, tenantId, ownerUserId);
            StoredRawDatasetFileResult storedFile = null;

            try
            {
                EnsureRequestMatchesDatasetVersion(request, datasetVersion);
                EnsureDatasetVersionCanStoreFile(datasetVersion);

                storedFile = await _rawDatasetStorage.StoreAsync(new StoreRawDatasetFileRequest
                {
                    TenantId = tenantId,
                    DatasetId = datasetVersion.DatasetId,
                    DatasetVersionId = datasetVersion.Id,
                    OriginalFileName = Path.GetFileName(request.OriginalFileName.Trim()),
                    ContentType = request.ContentType?.Trim(),
                    Content = request.Content,
                    FileKind = request.FileKind
                });

                await _datasetFileRepository.InsertAsync(new DatasetFile
                {
                    TenantId = tenantId,
                    DatasetVersionId = datasetVersion.Id,
                    StorageProvider = storedFile.StorageProvider,
                    StorageKey = storedFile.StorageKey,
                    OriginalFileName = storedFile.OriginalFileName,
                    ContentType = request.ContentType?.Trim(),
                    SizeBytes = storedFile.SizeBytes,
                    ChecksumSha256 = storedFile.ChecksumSha256
                });

                return storedFile;
            }
            catch (Exception exception) when (storedFile != null)
            {
                try
                {
                    await DeleteStoredFileAsync(storedFile);
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException("Dataset version file storage failed and cleanup also failed.", exception, cleanupException);
                }

                throw;
            }
        }

        private int GetRequiredTenantId()
        {
            if (!_abpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset file storage operations.");
            }

            return _abpSession.TenantId.Value;
        }

        private static void ValidateRequest(StoreRawDatasetFileRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.DatasetId <= 0)
            {
                throw new UserFriendlyException("A valid dataset identifier is required for dataset file storage.");
            }

            if (request.DatasetVersionId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version identifier is required for dataset file storage.");
            }

            if (request.OriginalFileName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("The original file name is required for dataset file storage.");
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new UserFriendlyException("A readable content stream is required for dataset file storage.");
            }
        }

        private static void EnsureRequestMatchesDatasetVersion(StoreRawDatasetFileRequest request, DatasetVersion datasetVersion)
        {
            if (request.DatasetId != datasetVersion.DatasetId)
            {
                throw new UserFriendlyException("The dataset version does not belong to the specified dataset.");
            }
        }

        private static void EnsureDatasetVersionCanStoreFile(DatasetVersion datasetVersion)
        {
            if (datasetVersion.RawFile != null)
            {
                throw new UserFriendlyException("A dataset file already exists for the specified dataset version.");
            }
        }

        /// <summary>
        /// Deletes a stored file using its logical storage reference.
        /// </summary>
        private Task DeleteStoredFileAsync(StoredRawDatasetFileResult storedFile)
        {
            return _rawDatasetStorage.DeleteAsync(new DeleteRawDatasetFileRequest
            {
                StorageProvider = storedFile.StorageProvider,
                StorageKey = storedFile.StorageKey
            });
        }
    }
}
