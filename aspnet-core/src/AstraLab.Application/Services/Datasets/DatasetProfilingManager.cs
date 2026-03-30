using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Profiling;
using AstraLab.Services.Datasets.Storage;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Orchestrates profiling for tenant-owned dataset versions and persists the current snapshot.
    /// </summary>
    public class DatasetProfilingManager : AstraLabAppServiceBase, IDatasetProfilingManager, ITransientDependency
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<DatasetColumnProfile, long> _datasetColumnProfileRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IRawDatasetStorage _rawDatasetStorage;
        private readonly IDatasetProfiler _datasetProfiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetProfilingManager"/> class.
        /// </summary>
        public DatasetProfilingManager(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<DatasetColumnProfile, long> datasetColumnProfileRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IRawDatasetStorage rawDatasetStorage,
            IDatasetProfiler datasetProfiler)
        {
            _datasetRepository = datasetRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetColumnProfileRepository = datasetColumnProfileRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _rawDatasetStorage = rawDatasetStorage;
            _datasetProfiler = datasetProfiler;
        }

        /// <summary>
        /// Profiles the specified dataset version and persists the current snapshot.
        /// </summary>
        public async Task<DatasetProfileDto> ProfileAsync(long datasetVersionId)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(datasetVersion.DatasetId, tenantId, ownerUserId);

            if (datasetVersion.RawFile == null)
            {
                throw new UserFriendlyException("The dataset version does not have a stored raw file to profile.");
            }

            var datasetColumns = await _datasetColumnRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            dataset.Status = DatasetStatus.Profiling;
            await CurrentUnitOfWork.SaveChangesAsync();

            try
            {
                ProfileDatasetVersionResult profilingResult;
                using (var contentStream = await _rawDatasetStorage.OpenReadAsync(new OpenReadRawDatasetFileRequest
                {
                    StorageProvider = datasetVersion.RawFile.StorageProvider,
                    StorageKey = datasetVersion.RawFile.StorageKey
                }))
                {
                    profilingResult = await _datasetProfiler.ProfileAsync(new ProfileDatasetVersionRequest
                    {
                        DatasetFormat = dataset.SourceFormat,
                        Columns = datasetColumns
                            .Select(item => new ProfileDatasetColumnRequest
                            {
                                DatasetColumnId = item.Id,
                                Name = item.Name,
                                Ordinal = item.Ordinal
                            })
                            .ToList(),
                        Content = contentStream
                    });
                }

                await ReplaceCurrentProfileSnapshotAsync(tenantId, datasetVersion, datasetColumns, profilingResult);

                dataset.Status = DatasetStatus.Ready;
                datasetVersion.Status = DatasetVersionStatus.Active;
                datasetVersion.RowCount = ToIntValue(profilingResult.RowCount);
                await CurrentUnitOfWork.SaveChangesAsync();

                return await BuildDatasetProfileDtoAsync(datasetVersion.Id, tenantId);
            }
            catch
            {
                dataset.Status = DatasetStatus.Failed;
                datasetVersion.Status = DatasetVersionStatus.Failed;
                await CurrentUnitOfWork.SaveChangesAsync();
                throw;
            }
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset profiling operations.");
            }

            return AbpSession.TenantId.Value;
        }

        /// <summary>
        /// Replaces the current persisted profiling snapshot and synchronizes legacy summary fields.
        /// </summary>
        private async Task ReplaceCurrentProfileSnapshotAsync(
            int tenantId,
            DatasetVersion datasetVersion,
            System.Collections.Generic.IReadOnlyList<DatasetColumn> datasetColumns,
            ProfileDatasetVersionResult profilingResult)
        {
            var existingProfile = await _datasetProfileRepository.GetAll()
                .Include(item => item.ColumnProfiles)
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                .FirstOrDefaultAsync();

            if (existingProfile != null)
            {
                foreach (var existingColumnProfile in existingProfile.ColumnProfiles.ToList())
                {
                    await _datasetColumnProfileRepository.HardDeleteAsync(existingColumnProfile);
                }

                await _datasetProfileRepository.HardDeleteAsync(existingProfile);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var datasetProfile = await _datasetProfileRepository.InsertAsync(new DatasetProfile
            {
                TenantId = tenantId,
                DatasetVersionId = datasetVersion.Id,
                RowCount = profilingResult.RowCount,
                DuplicateRowCount = profilingResult.DuplicateRowCount,
                DataHealthScore = profilingResult.DataHealthScore,
                SummaryJson = profilingResult.SummaryJson
            });

            await CurrentUnitOfWork.SaveChangesAsync();

            var profilingColumnsById = profilingResult.Columns.ToDictionary(item => item.DatasetColumnId);
            foreach (var datasetColumn in datasetColumns)
            {
                if (!profilingColumnsById.TryGetValue(datasetColumn.Id, out var profiledColumn))
                {
                    continue;
                }

                datasetColumn.DataType = profiledColumn.InferredDataType;
                datasetColumn.IsDataTypeInferred = true;
                datasetColumn.NullCount = profiledColumn.NullCount;
                datasetColumn.DistinctCount = profiledColumn.DistinctCount;

                await _datasetColumnProfileRepository.InsertAsync(new DatasetColumnProfile
                {
                    TenantId = tenantId,
                    DatasetProfileId = datasetProfile.Id,
                    DatasetColumnId = datasetColumn.Id,
                    InferredDataType = profiledColumn.InferredDataType,
                    NullCount = profiledColumn.NullCount,
                    DistinctCount = profiledColumn.DistinctCount,
                    StatisticsJson = profiledColumn.StatisticsJson
                });
            }
        }

        private async Task<DatasetProfileDto> BuildDatasetProfileDtoAsync(long datasetVersionId, int tenantId)
        {
            var datasetProfile = await _datasetProfileRepository.GetAll()
                .Include(item => item.ColumnProfiles)
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .FirstAsync();

            return new DatasetProfileDto
            {
                Id = datasetProfile.Id,
                DatasetVersionId = datasetProfile.DatasetVersionId,
                RowCount = datasetProfile.RowCount,
                DuplicateRowCount = datasetProfile.DuplicateRowCount,
                DataHealthScore = datasetProfile.DataHealthScore,
                SummaryJson = datasetProfile.SummaryJson,
                CreationTime = datasetProfile.CreationTime,
                ColumnProfiles = datasetProfile.ColumnProfiles
                    .OrderBy(item => item.DatasetColumnId)
                    .Select(MapDatasetColumnProfileDto)
                    .ToList()
            };
        }

        private static DatasetColumnProfileDto MapDatasetColumnProfileDto(DatasetColumnProfile datasetColumnProfile)
        {
            return new DatasetColumnProfileDto
            {
                Id = datasetColumnProfile.Id,
                DatasetProfileId = datasetColumnProfile.DatasetProfileId,
                DatasetColumnId = datasetColumnProfile.DatasetColumnId,
                InferredDataType = datasetColumnProfile.InferredDataType,
                NullCount = datasetColumnProfile.NullCount,
                NullPercentage = DatasetProfileSerialization.ReadNullPercentage(datasetColumnProfile.StatisticsJson),
                DistinctCount = datasetColumnProfile.DistinctCount,
                StatisticsJson = datasetColumnProfile.StatisticsJson,
                CreationTime = datasetColumnProfile.CreationTime
            };
        }

        private static int ToIntValue(long value)
        {
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }
    }
}
