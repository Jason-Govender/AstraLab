using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Profiling;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Exposes explicit workflows for executing dataset transformation pipelines.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetTransformationAppService : AstraLabAppServiceBase, IDatasetTransformationAppService
    {
        private readonly IDatasetTransformationManager _datasetTransformationManager;
        private readonly IRepository<DatasetTransformation, long> _datasetTransformationRepository;
        private readonly IRepository<DatasetColumn, long> _datasetColumnRepository;
        private readonly IRepository<DatasetFile, long> _datasetFileRepository;
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransformationAppService"/> class.
        /// </summary>
        public DatasetTransformationAppService(
            IDatasetTransformationManager datasetTransformationManager,
            IRepository<DatasetTransformation, long> datasetTransformationRepository,
            IRepository<DatasetColumn, long> datasetColumnRepository,
            IRepository<DatasetFile, long> datasetFileRepository,
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _datasetTransformationManager = datasetTransformationManager;
            _datasetTransformationRepository = datasetTransformationRepository;
            _datasetColumnRepository = datasetColumnRepository;
            _datasetFileRepository = datasetFileRepository;
            _datasetProfileRepository = datasetProfileRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Gets the persisted transformation history for the requested dataset.
        /// </summary>
        public async Task<DatasetTransformationHistoryDto> GetHistoryAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var dataset = await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.Id, tenantId, ownerUserId);

            var transformations = await _datasetTransformationRepository.GetAll()
                .Include(item => item.SourceDatasetVersion)
                .Include(item => item.ResultDatasetVersion)
                .Where(item =>
                    item.TenantId == tenantId &&
                    ((item.SourceDatasetVersion != null && item.SourceDatasetVersion.DatasetId == dataset.Id) ||
                     (item.ResultDatasetVersionId.HasValue && item.ResultDatasetVersion.DatasetId == dataset.Id)))
                .OrderByDescending(item => item.ExecutedAt)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            return new DatasetTransformationHistoryDto
            {
                DatasetId = dataset.Id,
                CurrentVersionId = dataset.CurrentVersionId,
                Items = transformations.Select(item => new DatasetTransformationHistoryItemDto
                {
                    Transformation = ObjectMapper.Map<DatasetTransformationDto>(item),
                    SourceVersion = MapToDatasetVersionSummaryDto(item.SourceDatasetVersion),
                    ResultVersion = item.ResultDatasetVersion == null ? null : MapToDatasetVersionSummaryDto(item.ResultDatasetVersion)
                }).ToList()
            };
        }

        /// <summary>
        /// Gets the detailed metadata for a processed dataset version.
        /// </summary>
        public async Task<ProcessedDatasetVersionDto> GetProcessedVersionAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.Id, tenantId, ownerUserId);

            if (datasetVersion.VersionType != DatasetVersionType.Processed)
            {
                throw new UserFriendlyException("Only processed dataset versions can be retrieved from the processed-version endpoint.");
            }

            var columns = await _datasetColumnRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == datasetVersion.Id &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .OrderBy(item => item.Ordinal)
                .ToListAsync();

            var storedFile = await _datasetFileRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == datasetVersion.Id &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            var datasetProfile = await _datasetProfileRepository.GetAll()
                .Include(item => item.ColumnProfiles)
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersion.Id)
                .FirstOrDefaultAsync();

            var producingTransformation = await _datasetTransformationRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.ResultDatasetVersionId == datasetVersion.Id &&
                    item.SourceDatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            return new ProcessedDatasetVersionDto
            {
                Version = MapToDatasetVersionDetailsDto(datasetVersion, storedFile, datasetProfile),
                Columns = ObjectMapper.Map<System.Collections.Generic.List<DatasetColumnDto>>(columns),
                ProducedByTransformation = producingTransformation == null ? null : ObjectMapper.Map<DatasetTransformationDto>(producingTransformation)
            };
        }

        /// <summary>
        /// Executes an ordered transformation pipeline against a source dataset version.
        /// </summary>
        public Task<TransformDatasetVersionResultDto> TransformAsync(TransformDatasetVersionRequest input)
        {
            return _datasetTransformationManager.TransformAsync(input);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset transformation operations.");
            }

            return AbpSession.TenantId.Value;
        }

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

        private static DatasetVersionDetailsDto MapToDatasetVersionDetailsDto(DatasetVersion datasetVersion, DatasetFile storedFile, DatasetProfile datasetProfile)
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
                                NullPercentage = DatasetProfileSerialization.ReadNullPercentage(item.StatisticsJson),
                                DistinctCount = item.DistinctCount,
                                StatisticsJson = item.StatisticsJson,
                                CreationTime = item.CreationTime
                            })
                            .ToList()
                    },
                RawFile = storedFile == null
                    ? null
                    : new DatasetFileSummaryDto
                    {
                        OriginalFileName = storedFile.OriginalFileName,
                        ContentType = storedFile.ContentType,
                        SizeBytes = storedFile.SizeBytes,
                        CreationTime = storedFile.CreationTime
                    }
            };
        }
    }
}
