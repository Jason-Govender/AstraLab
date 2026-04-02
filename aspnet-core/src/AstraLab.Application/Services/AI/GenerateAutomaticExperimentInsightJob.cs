using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.ML;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Generates best-effort automatic AI insights for completed machine learning experiments.
    /// </summary>
    public class GenerateAutomaticExperimentInsightJob :
        AsyncBackgroundJob<GenerateAutomaticExperimentInsightJobArgs>,
        ITransientDependency
    {
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateAutomaticExperimentInsightJob"/> class.
        /// </summary>
        public GenerateAutomaticExperimentInsightJob(
            IRepository<MLExperiment, long> mlExperimentRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IAiDatasetResponseGenerator aiDatasetResponseGenerator)
        {
            _mlExperimentRepository = mlExperimentRepository;
            _aiResponseRepository = aiResponseRepository;
            _aiDatasetResponseGenerator = aiDatasetResponseGenerator;
        }

        /// <summary>
        /// Generates the automatic insight when the experiment is still current, completed, and not already processed.
        /// </summary>
        [UnitOfWork]
        public override async Task ExecuteAsync(GenerateAutomaticExperimentInsightJobArgs args)
        {
            var experiment = await _mlExperimentRepository.GetAll()
                .Include(item => item.Model)
                .Where(item =>
                    item.Id == args.MLExperimentId &&
                    item.TenantId == args.TenantId)
                .SingleOrDefaultAsync();

            if (experiment == null ||
                experiment.DatasetVersionId != args.DatasetVersionId ||
                experiment.Status != MLExperimentStatus.Completed ||
                experiment.Model == null)
            {
                return;
            }

            var existingAutomaticInsight = await _aiResponseRepository.GetAll()
                .Where(item =>
                    item.TenantId == args.TenantId &&
                    item.MLExperimentId == args.MLExperimentId &&
                    item.ResponseType == AIResponseType.Insight)
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            if (existingAutomaticInsight.Any(item =>
                    AiAutomaticInsightMetadata.IsAutomaticExperimentInsight(item.MetadataJson, args.MLExperimentId)))
            {
                return;
            }

            await _aiDatasetResponseGenerator.GenerateAutomaticExperimentInsightAsync(
                args.MLExperimentId,
                args.TenantId,
                args.OwnerUserId);
        }
    }
}
