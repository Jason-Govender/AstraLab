using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.ML.Dto;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Exposes machine learning experiment workflows for tenant-owned dataset versions.
    /// </summary>
    public interface IMLExperimentAppService : IApplicationService
    {
        /// <summary>
        /// Creates and dispatches a machine learning experiment.
        /// </summary>
        Task<MlExperimentDto> CreateAsync(CreateMlExperimentRequest input);

        /// <summary>
        /// Gets a single machine learning experiment.
        /// </summary>
        Task<MlExperimentDto> GetAsync(EntityDto<long> input);

        /// <summary>
        /// Lists machine learning experiments for a dataset version.
        /// </summary>
        Task<ListResultDto<MlExperimentDto>> GetByDatasetVersionAsync(EntityDto<long> input);

        /// <summary>
        /// Cancels a pending machine learning experiment.
        /// </summary>
        Task<MlExperimentDto> CancelAsync(EntityDto<long> input);

        /// <summary>
        /// Retries an existing machine learning experiment by creating a new experiment record.
        /// </summary>
        Task<MlExperimentDto> RetryAsync(EntityDto<long> input);
    }
}
