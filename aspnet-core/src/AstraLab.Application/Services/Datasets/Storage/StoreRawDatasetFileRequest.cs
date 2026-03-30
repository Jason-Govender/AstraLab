using System.IO;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Carries the data needed to store an immutable raw dataset file.
    /// </summary>
    public class StoreRawDatasetFileRequest
    {
        /// <summary>
        /// Gets or sets the tenant that owns the raw file.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset identifier associated with the raw file.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that owns the raw file.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the optional content type of the uploaded file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content stream to store.
        /// </summary>
        public Stream Content { get; set; }
    }
}
