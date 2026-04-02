using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.UI;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.ML.Storage;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Validates ownership and opens persisted ML artifacts for download.
    /// </summary>
    public class MLArtifactAccessService : IMLArtifactAccessService, ITransientDependency
    {
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IMLArtifactStorage _mlArtifactStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLArtifactAccessService"/> class.
        /// </summary>
        public MLArtifactAccessService(
            IRepository<MLExperiment, long> mlExperimentRepository,
            IMLArtifactStorage mlArtifactStorage)
        {
            _mlExperimentRepository = mlExperimentRepository;
            _mlArtifactStorage = mlArtifactStorage;
        }

        /// <summary>
        /// Opens a tenant-owned ML artifact for download.
        /// </summary>
        [UnitOfWork]
        public async Task<MLArtifactDownloadResult> OpenDownloadAsync(long mlExperimentId, int tenantId, long ownerUserId)
        {
            var experiment = await GetValidatedExperimentAsync(mlExperimentId, tenantId, ownerUserId);
            var model = experiment.Model;

            if (model == null || model.ArtifactStorageProvider.IsNullOrWhiteSpace() || model.ArtifactStorageKey.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("The requested ML experiment does not have a stored artifact.");
            }

            try
            {
                var contentStream = await _mlArtifactStorage.OpenReadAsync(new OpenReadMlArtifactRequest
                {
                    StorageProvider = model.ArtifactStorageProvider,
                    StorageKey = model.ArtifactStorageKey
                });

                return new MLArtifactDownloadResult
                {
                    Content = contentStream,
                    FileName = BuildFileName(experiment.Id, model.ModelType, model.ArtifactStorageKey),
                    ContentType = null
                };
            }
            catch (FileNotFoundException)
            {
                throw new UserFriendlyException("The requested ML experiment does not have a stored artifact.");
            }
        }

        /// <summary>
        /// Gets a tenant-owned ML experiment scoped to the current dataset owner.
        /// </summary>
        private async Task<MLExperiment> GetValidatedExperimentAsync(long mlExperimentId, int tenantId, long ownerUserId)
        {
            var experiment = await _mlExperimentRepository.GetAll()
                .Include(item => item.Model)
                .Where(item =>
                    item.Id == mlExperimentId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            return experiment;
        }

        /// <summary>
        /// Builds a deterministic artifact download file name.
        /// </summary>
        private static string BuildFileName(long experimentId, string modelType, string artifactStorageKey)
        {
            var normalizedModelType = SanitizeFileNameSegment(modelType);
            var extension = Path.GetExtension(artifactStorageKey);
            if (extension.IsNullOrWhiteSpace())
            {
                extension = ".bin";
            }

            return $"ml-experiment-{experimentId}-{normalizedModelType}{extension}";
        }

        /// <summary>
        /// Normalizes a user-facing filename segment to avoid invalid file characters.
        /// </summary>
        private static string SanitizeFileNameSegment(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return "model";
            }

            var invalidCharacters = Path.GetInvalidFileNameChars();
            var normalizedCharacters = value.Trim()
                .Select(character => invalidCharacters.Contains(character) || char.IsWhiteSpace(character)
                    ? '-'
                    : char.ToLowerInvariant(character))
                .ToArray();

            var normalizedValue = new string(normalizedCharacters).Trim('-');
            return string.IsNullOrWhiteSpace(normalizedValue) ? "model" : normalizedValue;
        }
    }
}
