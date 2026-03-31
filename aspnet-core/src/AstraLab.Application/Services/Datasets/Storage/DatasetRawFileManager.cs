using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Orchestrates immutable raw file storage for tenant-owned dataset versions.
    /// </summary>
    public class DatasetRawFileManager : IDatasetRawFileManager, ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IDatasetVersionFileManager _datasetVersionFileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetRawFileManager"/> class.
        /// </summary>
        public DatasetRawFileManager(
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IDatasetVersionFileManager datasetVersionFileManager)
        {
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _datasetVersionFileManager = datasetVersionFileManager;
        }

        /// <summary>
        /// Stores a raw dataset file for a tenant-owned raw dataset version and persists its logical reference.
        /// </summary>
        public async Task<StoredRawDatasetFileResult> StoreForVersionAsync(StoreRawDatasetFileRequest request)
        {
            ValidateRequest(request);

            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                var tenantId = GetRequiredTenantId();
                var ownerUserId = _abpSession.GetUserId();
                var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(request.DatasetVersionId, tenantId, ownerUserId);
                EnsureRequestMatchesDatasetVersion(request, datasetVersion);
                EnsureDatasetVersionCanStoreRawFile(datasetVersion);

                var storedRawFile = await _datasetVersionFileManager.StoreForVersionAsync(new StoreRawDatasetFileRequest
                {
                    DatasetId = datasetVersion.DatasetId,
                    DatasetVersionId = datasetVersion.Id,
                    OriginalFileName = Path.GetFileName(request.OriginalFileName.Trim()),
                    ContentType = request.ContentType?.Trim(),
                    Content = request.Content,
                    FileKind = DatasetVersionFileKind.Raw
                });

                await unitOfWork.CompleteAsync();
                return storedRawFile;
            }
        }

        private int GetRequiredTenantId()
        {
            if (!_abpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for raw dataset storage operations.");
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
                throw new UserFriendlyException("A valid dataset identifier is required for raw dataset storage.");
            }

            if (request.DatasetVersionId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version identifier is required for raw dataset storage.");
            }

            if (request.OriginalFileName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("The original file name is required for raw dataset storage.");
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new UserFriendlyException("A readable content stream is required for raw dataset storage.");
            }
        }

        private static void EnsureRequestMatchesDatasetVersion(StoreRawDatasetFileRequest request, DatasetVersion datasetVersion)
        {
            if (request.DatasetId != datasetVersion.DatasetId)
            {
                throw new UserFriendlyException("The dataset version does not belong to the specified dataset.");
            }
        }

        private static void EnsureDatasetVersionCanStoreRawFile(DatasetVersion datasetVersion)
        {
            if (datasetVersion.VersionType != DatasetVersionType.Raw)
            {
                throw new UserFriendlyException("Only raw dataset versions can store immutable raw files.");
            }

            if (datasetVersion.RawFile != null)
            {
                throw new UserFriendlyException("A raw dataset file already exists for the specified dataset version.");
            }
        }
    }
}
