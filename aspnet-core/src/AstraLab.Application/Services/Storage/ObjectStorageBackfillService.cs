using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Storage
{
    /// <summary>
    /// Copies legacy local dataset files and ML artifacts into the configured object-storage providers.
    /// </summary>
    public class ObjectStorageBackfillService : IObjectStorageBackfillService, ITransientDependency
    {
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<MLModel, long> _mlModelRepository;
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IMLArtifactStorage _mlArtifactStorage;
        private readonly DatasetStorageOptions _datasetStorageOptions;
        private readonly MLExecutionOptions _mlExecutionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectStorageBackfillService"/> class.
        /// </summary>
        public ObjectStorageBackfillService(
            IRepository<DatasetFile, long> datasetFileRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<MLModel, long> mlModelRepository,
            IRawDatasetStorage rawDatasetStorage,
            IMLArtifactStorage mlArtifactStorage,
            DatasetStorageOptions datasetStorageOptions,
            MLExecutionOptions mlExecutionOptions)
        {
            _datasetFileRepository = datasetFileRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _mlModelRepository = mlModelRepository;
            _rawDatasetStorage = rawDatasetStorage;
            _mlArtifactStorage = mlArtifactStorage;
            _datasetStorageOptions = datasetStorageOptions;
            _mlExecutionOptions = mlExecutionOptions;
        }

        /// <summary>
        /// Backfills legacy local-storage dataset files and ML artifacts.
        /// </summary>
        public async Task<ObjectStorageBackfillResult> BackfillAsync(int batchSize = 100)
        {
            var result = new ObjectStorageBackfillResult();
            var normalizedBatchSize = batchSize <= 0 ? 100 : batchSize;

            if (!string.Equals(_datasetStorageOptions.DefaultProvider, "local-filesystem", StringComparison.OrdinalIgnoreCase))
            {
                await BackfillDatasetFilesAsync(normalizedBatchSize, result);
            }

            if (!string.Equals(_mlExecutionOptions.DefaultArtifactStorageProvider, "local-filesystem", StringComparison.OrdinalIgnoreCase))
            {
                await BackfillMlArtifactsAsync(normalizedBatchSize, result);
            }

            return result;
        }

        private async Task BackfillDatasetFilesAsync(int batchSize, ObjectStorageBackfillResult result)
        {
            var datasetFiles = await _datasetFileRepository.GetAll()
                .Where(item => item.StorageProvider == "local-filesystem")
                .OrderBy(item => item.Id)
                .Take(batchSize)
                .ToListAsync();

            foreach (var datasetFile in datasetFiles)
            {
                try
                {
                    var datasetVersion = await _datasetVersionRepository.GetAsync(datasetFile.DatasetVersionId);

                    using (var sourceStream = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
                    {
                        StorageProvider = datasetFile.StorageProvider,
                        StorageKey = datasetFile.StorageKey
                    }))
                    {
                        StoredRawDatasetFileResult storedFile;
                        try
                        {
                            storedFile = await _rawDatasetStorage.StoreAsync(new StoreRawDatasetFileRequest
                            {
                                TenantId = datasetFile.TenantId,
                                DatasetId = datasetVersion.DatasetId,
                                DatasetVersionId = datasetFile.DatasetVersionId,
                                OriginalFileName = datasetFile.OriginalFileName,
                                ContentType = datasetFile.ContentType,
                                Content = sourceStream,
                                FileKind = datasetVersion.VersionType == DatasetVersionType.Processed
                                    ? DatasetVersionFileKind.Processed
                                    : DatasetVersionFileKind.Raw
                            });
                        }
                        catch (IOException)
                        {
                            storedFile = new StoredRawDatasetFileResult
                            {
                                StorageProvider = _datasetStorageOptions.DefaultProvider,
                                StorageKey = datasetFile.StorageKey,
                                OriginalFileName = datasetFile.OriginalFileName,
                                SizeBytes = datasetFile.SizeBytes,
                                ChecksumSha256 = datasetFile.ChecksumSha256
                            };
                        }

                        datasetFile.StorageProvider = storedFile.StorageProvider;
                        datasetFile.StorageKey = storedFile.StorageKey;
                        datasetFile.SizeBytes = storedFile.SizeBytes;
                        datasetFile.ChecksumSha256 = storedFile.ChecksumSha256;
                        result.MigratedDatasetFileCount++;
                    }
                }
                catch (Exception exception)
                {
                    result.Errors.Add(string.Format("DatasetFile:{0}:{1}", datasetFile.Id, exception.Message));
                }
            }
        }

        private async Task BackfillMlArtifactsAsync(int batchSize, ObjectStorageBackfillResult result)
        {
            var models = await _mlModelRepository.GetAll()
                .Where(item =>
                    item.ArtifactStorageProvider == "local-filesystem" &&
                    item.ArtifactStorageKey != null &&
                    item.ArtifactStorageKey != string.Empty)
                .OrderBy(item => item.Id)
                .Take(batchSize)
                .ToListAsync();

            foreach (var model in models)
            {
                try
                {
                    using (var sourceStream = await _mlArtifactStorage.OpenReadAsync(new OpenReadMlArtifactRequest
                    {
                        StorageProvider = model.ArtifactStorageProvider,
                        StorageKey = model.ArtifactStorageKey
                    }))
                    {
                        var storedArtifact = await _mlArtifactStorage.StoreAsync(new StoreMlArtifactRequest
                        {
                            StorageKey = model.ArtifactStorageKey,
                            Content = sourceStream
                        });

                        model.ArtifactStorageProvider = storedArtifact.StorageProvider;
                        model.ArtifactStorageKey = storedArtifact.StorageKey;
                        result.MigratedMlArtifactCount++;
                    }
                }
                catch (Exception exception)
                {
                    result.Errors.Add(string.Format("MLModel:{0}:{1}", model.Id, exception.Message));
                }
            }
        }
    }
}
