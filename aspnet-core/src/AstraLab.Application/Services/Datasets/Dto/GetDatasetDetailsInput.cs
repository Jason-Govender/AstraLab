using Abp.Application.Services.Dto;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the request used to retrieve dataset details for the details page.
    /// </summary>
    public class GetDatasetDetailsInput
    {
        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the optional selected dataset version identifier.
        /// </summary>
        public long? SelectedVersionId { get; set; }
    }
}
