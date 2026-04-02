using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Generates a persisted automatic insight after profiling completes for a dataset version.
    /// </summary>
    public class GenerateAutomaticDatasetInsightJob : AsyncBackgroundJob<GenerateAutomaticDatasetInsightJobArgs>, ITransientDependency
    {
        private readonly IRepository<DatasetProfile, long> _datasetProfileRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateAutomaticDatasetInsightJob"/> class.
        /// </summary>
        public GenerateAutomaticDatasetInsightJob(
            IRepository<DatasetProfile, long> datasetProfileRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IAiDatasetResponseGenerator aiDatasetResponseGenerator)
        {
            _datasetProfileRepository = datasetProfileRepository;
            _aiResponseRepository = aiResponseRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _aiDatasetResponseGenerator = aiDatasetResponseGenerator;
        }

        /// <summary>
        /// Executes the automatic insight generation workflow when the queued profile is still current.
        /// </summary>
        public override async Task ExecuteAsync(GenerateAutomaticDatasetInsightJobArgs args)
        {
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(args.DatasetVersionId, args.TenantId, args.OwnerUserId);

            var currentProfile = await _datasetProfileRepository.GetAll()
                .Where(item => item.TenantId == args.TenantId && item.DatasetVersionId == args.DatasetVersionId)
                .SingleOrDefaultAsync();

            if (currentProfile == null || currentProfile.Id != args.DatasetProfileId)
            {
                return;
            }

            var existingInsightMetadata = await _aiResponseRepository.GetAll()
                .Where(item =>
                    item.TenantId == args.TenantId &&
                    item.DatasetVersionId == args.DatasetVersionId &&
                    item.ResponseType == AIResponseType.Insight)
                .Select(item => item.MetadataJson)
                .ToListAsync();

            if (existingInsightMetadata.Any(item => AiAutomaticInsightMetadata.IsAutomaticProfilingInsight(item, args.DatasetProfileId)))
            {
                return;
            }

            await _aiDatasetResponseGenerator.GenerateAutomaticInsightAsync(
                args.DatasetVersionId,
                args.DatasetProfileId,
                args.TenantId,
                args.OwnerUserId);
        }
    }
}
