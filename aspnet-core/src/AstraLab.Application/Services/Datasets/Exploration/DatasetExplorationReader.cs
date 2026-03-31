using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.Datasets.Transformations;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets.Exploration
{
    /// <summary>
    /// Loads stored dataset-version content for exploration workflows.
    /// </summary>
    public class DatasetExplorationReader : IDatasetExplorationReader, ITransientDependency
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IDatasetTabularDataCodec _datasetTabularDataCodec;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetExplorationReader"/> class.
        /// </summary>
        public DatasetExplorationReader(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IRawDatasetStorage rawDatasetStorage,
            IDatasetTabularDataCodec datasetTabularDataCodec)
        {
            _datasetRepository = datasetRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _rawDatasetStorage = rawDatasetStorage;
            _datasetTabularDataCodec = datasetTabularDataCodec;
        }

        /// <summary>
        /// Gets the ordered persisted columns for an owner-scoped dataset version.
        /// </summary>
        public async Task<List<DatasetColumn>> GetColumnsAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);

            return await _datasetColumnRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == datasetVersionId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();
        }

        /// <summary>
        /// Loads and parses a tenant-owned dataset version into the shared tabular model.
        /// </summary>
        public async Task<LoadedExplorationDataset> LoadAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            if (datasetVersion.RawFile == null)
            {
                throw new UserFriendlyException("The dataset version does not have a stored dataset file to explore.");
            }

            var dataset = await _datasetRepository.GetAsync(datasetVersion.DatasetId);
            var columns = await GetColumnsAsync(datasetVersionId, tenantId, ownerUserId);
            if (columns.Count == 0)
            {
                throw dataset.SourceFormat == DatasetFormat.Json
                    ? new UserFriendlyException("Only tabular JSON datasets can be explored.")
                    : new UserFriendlyException("Only tabular dataset versions with persisted columns can be explored.");
            }

            try
            {
                using (var stream = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
                {
                    StorageProvider = datasetVersion.RawFile.StorageProvider,
                    StorageKey = datasetVersion.RawFile.StorageKey
                }))
                {
                    var tabularDataset = await _datasetTabularDataCodec.ReadAsync(dataset.SourceFormat, columns, stream);
                    return new LoadedExplorationDataset
                    {
                        DatasetVersion = datasetVersion,
                        Columns = columns,
                        Dataset = tabularDataset
                    };
                }
            }
            catch (UserFriendlyException exception) when (dataset.SourceFormat == DatasetFormat.Json)
            {
                throw new UserFriendlyException("Only tabular JSON datasets can be explored.", exception);
            }
        }
    }
}
