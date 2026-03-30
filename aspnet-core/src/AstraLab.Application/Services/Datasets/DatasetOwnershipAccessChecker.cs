using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides owner-scoped dataset resource lookups for dataset application workflows.
    /// </summary>
    public class DatasetOwnershipAccessChecker : IDatasetOwnershipAccessChecker, ITransientDependency
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetOwnershipAccessChecker"/> class.
        /// </summary>
        public DatasetOwnershipAccessChecker(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
        }

        /// <summary>
        /// Gets a dataset for the specified tenant owner.
        /// </summary>
        public async Task<Dataset> GetDatasetForOwnerAsync(long datasetId, int tenantId, long ownerUserId)
        {
            var dataset = await _datasetRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.OwnerUserId == ownerUserId && item.Id == datasetId)
                .FirstOrDefaultAsync();

            if (dataset == null)
            {
                throw new EntityNotFoundException(typeof(Dataset), datasetId);
            }

            return dataset;
        }

        /// <summary>
        /// Gets a dataset version for the specified tenant owner.
        /// </summary>
        public async Task<DatasetVersion> GetDatasetVersionForOwnerAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var datasetVersion = await _datasetVersionRepository.GetAll()
                .Include(item => item.RawFile)
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.Id == datasetVersionId &&
                    item.Dataset.TenantId == tenantId &&
                    item.Dataset.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            if (datasetVersion == null)
            {
                throw new EntityNotFoundException(typeof(DatasetVersion), datasetVersionId);
            }

            return datasetVersion;
        }

        /// <summary>
        /// Gets a dataset column for the specified tenant owner.
        /// </summary>
        public async Task<DatasetColumn> GetDatasetColumnForOwnerAsync(long datasetColumnId, int tenantId, long ownerUserId)
        {
            var datasetColumn = await _datasetColumnRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.Id == datasetColumnId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            if (datasetColumn == null)
            {
                throw new EntityNotFoundException(typeof(DatasetColumn), datasetColumnId);
            }

            return datasetColumn;
        }
    }
}
