using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides ingestion-oriented dataset metadata workflows.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetAppService : AstraLabAppServiceBase, IDatasetAppService
    {
        private readonly IRepository<Dataset, long> _datasetRepository;
        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetAppService"/> class.
        /// </summary>
        public DatasetAppService(
            IRepository<Dataset, long> datasetRepository,
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetFile, long> datasetFileRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _datasetRepository = datasetRepository;
            _datasetVersionRepository = datasetVersionRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetFileRepository = datasetFileRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Creates a new dataset metadata record for the current tenant user.
        /// </summary>
        public async Task<DatasetDto> CreateAsync(CreateDatasetDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var dataset = ObjectMapper.Map<Dataset>(input);
            dataset.TenantId = tenantId;
            dataset.OwnerUserId = ownerUserId;
            dataset.Status = DatasetStatus.Uploaded;

            dataset = await _datasetRepository.InsertAsync(dataset);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DatasetDto>(dataset);
        }

        /// <summary>
        /// Gets a dataset for the current tenant.
        /// </summary>
        public async Task<DatasetDto> GetAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.Id, tenantId, ownerUserId);

            return ObjectMapper.Map<DatasetDto>(dataset);
        }

        /// <summary>
        /// Gets the composite dataset details payload for the current owner and tenant.
        /// </summary>
        public async Task<DatasetDetailsDto> GetDetailsAsync(GetDatasetDetailsInput input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.DatasetId, tenantId, ownerUserId);

            var versions = await _datasetVersionRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetId == dataset.Id &&
                    item.Dataset.OwnerUserId == ownerUserId)
                .OrderByDescending(item => item.VersionNumber)
                .ToListAsync();

            var selectedVersionId = input.SelectedVersionId ?? dataset.CurrentVersionId;
            DatasetVersion selectedVersion = null;

            if (selectedVersionId.HasValue)
            {
                selectedVersion = versions.FirstOrDefault(item => item.Id == selectedVersionId.Value);
                if (selectedVersion == null)
                {
                    throw new UserFriendlyException("The selected dataset version does not belong to the requested dataset.");
                }
            }

            var columns = selectedVersion == null
                ? new System.Collections.Generic.List<DatasetColumn>()
                : await _datasetColumnRepository.GetAll()
                    .Where(item =>
                        item.TenantId == tenantId &&
                        item.DatasetVersionId == selectedVersion.Id &&
                        item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                    .OrderBy(item => item.Ordinal)
                    .ToListAsync();

            DatasetFile rawFile = null;
            DatasetProfile datasetProfile = null;
            if (selectedVersion != null)
            {
                rawFile = await _datasetFileRepository.GetAll()
                    .Where(item =>
                        item.TenantId == tenantId &&
                        item.DatasetVersionId == selectedVersion.Id &&
                        item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                    .FirstOrDefaultAsync();

                datasetProfile = await _datasetProfileRepository.GetAll()
                    .Include(item => item.ColumnProfiles)
                    .Where(item => item.TenantId == tenantId && item.DatasetVersionId == selectedVersion.Id)
                    .FirstOrDefaultAsync();
            }

            return new DatasetDetailsDto
            {
                Dataset = ObjectMapper.Map<DatasetDto>(dataset),
                Versions = versions.Select(MapToDatasetVersionSummaryDto).ToList(),
                SelectedVersion = selectedVersion == null ? null : MapToDatasetVersionDetailsDto(selectedVersion, rawFile, datasetProfile),
                Columns = ObjectMapper.Map<System.Collections.Generic.List<DatasetColumnDto>>(columns)
            };
        }

        /// <summary>
        /// Gets paged datasets for the current tenant.
        /// </summary>
        public async Task<PagedResultDto<DatasetListItemDto>> GetAllAsync(PagedDatasetResultRequestDto input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();

            var currentVersions = _datasetVersionRepository.GetAll()
                .Where(item => item.TenantId == tenantId);

            var query =
                from dataset in _datasetRepository.GetAll()
                where dataset.TenantId == tenantId && dataset.OwnerUserId == ownerUserId
                join currentVersion in currentVersions
                    on dataset.CurrentVersionId equals (long?)currentVersion.Id into currentVersionJoin
                from currentVersion in currentVersionJoin.DefaultIfEmpty()
                where input.Keyword.IsNullOrWhiteSpace() ||
                      dataset.Name.Contains(input.Keyword) ||
                      (dataset.Description != null && dataset.Description.Contains(input.Keyword))
                where !input.Status.HasValue || dataset.Status == input.Status.Value
                select new DatasetListItemDto
                {
                    Id = dataset.Id,
                    Name = dataset.Name,
                    Description = dataset.Description,
                    SourceFormat = dataset.SourceFormat,
                    Status = dataset.Status,
                    OriginalFileName = dataset.OriginalFileName,
                    CreationTime = dataset.CreationTime,
                    CurrentVersionId = dataset.CurrentVersionId,
                    CurrentVersionNumber = currentVersion != null ? (int?)currentVersion.VersionNumber : null,
                    CurrentVersionStatus = currentVersion != null ? (DatasetVersionStatus?)currentVersion.Status : null,
                    ColumnCount = currentVersion != null ? currentVersion.ColumnCount : null
                };

            var totalCount = await query.CountAsync();

            var datasets = await query
                .OrderByDescending(item => item.CreationTime)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DatasetListItemDto>(totalCount, datasets);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset operations.");
            }

            return AbpSession.TenantId.Value;
        }

        /// <summary>
        /// Maps a dataset version to the lightweight summary shape used by the details page.
        /// </summary>
        private static DatasetVersionSummaryDto MapToDatasetVersionSummaryDto(DatasetVersion datasetVersion)
        {
            return new DatasetVersionSummaryDto
            {
                Id = datasetVersion.Id,
                VersionNumber = datasetVersion.VersionNumber,
                VersionType = datasetVersion.VersionType,
                Status = datasetVersion.Status,
                CreationTime = datasetVersion.CreationTime,
                ColumnCount = datasetVersion.ColumnCount
            };
        }

        /// <summary>
        /// Maps the selected dataset version and optional raw file into the details shape.
        /// </summary>
        private static DatasetVersionDetailsDto MapToDatasetVersionDetailsDto(DatasetVersion datasetVersion, DatasetFile rawFile, DatasetProfile datasetProfile)
        {
            return new DatasetVersionDetailsDto
            {
                Id = datasetVersion.Id,
                DatasetId = datasetVersion.DatasetId,
                VersionNumber = datasetVersion.VersionNumber,
                VersionType = datasetVersion.VersionType,
                Status = datasetVersion.Status,
                ParentVersionId = datasetVersion.ParentVersionId,
                RowCount = datasetVersion.RowCount,
                ColumnCount = datasetVersion.ColumnCount,
                SchemaJson = datasetVersion.SchemaJson,
                SizeBytes = datasetVersion.SizeBytes,
                CreationTime = datasetVersion.CreationTime,
                Profile = datasetProfile == null
                    ? null
                    : new DatasetProfileDto
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
                            .Select(item => new DatasetColumnProfileDto
                            {
                                Id = item.Id,
                                DatasetProfileId = item.DatasetProfileId,
                                DatasetColumnId = item.DatasetColumnId,
                                InferredDataType = item.InferredDataType,
                                NullCount = item.NullCount,
                                NullPercentage = Profiling.DatasetProfileSerialization.ReadNullPercentage(item.StatisticsJson),
                                DistinctCount = item.DistinctCount,
                                StatisticsJson = item.StatisticsJson,
                                CreationTime = item.CreationTime
                            })
                            .ToList(),
                    },
                RawFile = rawFile == null
                    ? null
                    : new DatasetFileSummaryDto
                    {
                        OriginalFileName = rawFile.OriginalFileName,
                        ContentType = rawFile.ContentType,
                        SizeBytes = rawFile.SizeBytes,
                        CreationTime = rawFile.CreationTime
                    }
            };
        }
    }
}
