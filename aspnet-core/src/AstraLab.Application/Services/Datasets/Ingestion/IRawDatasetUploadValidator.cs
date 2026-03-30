using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Validates raw dataset uploads at the ingestion boundary before persistence begins.
    /// </summary>
    public interface IRawDatasetUploadValidator
    {
        /// <summary>
        /// Validates the upload request and returns the inferred dataset format.
        /// </summary>
        Task<DatasetFormat> ValidateAsync(UploadRawDatasetRequest request);
    }
}
