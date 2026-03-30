using System.Threading.Tasks;
using Abp.Application.Services;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides ingestion workflows for validated raw dataset uploads.
    /// </summary>
    public interface IDatasetIngestionAppService : IApplicationService
    {
        /// <summary>
        /// Uploads a validated raw dataset file and creates its initial dataset records.
        /// </summary>
        Task<UploadedRawDatasetDto> UploadRawAsync(UploadRawDatasetRequest input);
    }
}
