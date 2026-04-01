using System;
using System.IO;

namespace AstraLab.Services.Datasets.Storage
{
    /// <summary>
    /// Builds normalized logical storage keys for immutable dataset version files.
    /// </summary>
    public static class DatasetStorageKeyBuilder
    {
        /// <summary>
        /// Builds the logical storage key for the supplied dataset file request.
        /// </summary>
        public static string BuildStorageKey(StoreRawDatasetFileRequest request, string checksumSha256)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(checksumSha256))
            {
                throw new ArgumentException("A checksum is required to build a dataset storage key.", nameof(checksumSha256));
            }

            var extension = Path.GetExtension(Path.GetFileName(request.OriginalFileName.Trim())).ToLowerInvariant();

            return string.Format(
                "tenants/{0}/datasets/{1}/versions/{2}/{3}/{4}{5}",
                request.TenantId,
                request.DatasetId,
                request.DatasetVersionId,
                GetStorageFolderName(request.FileKind),
                checksumSha256,
                extension);
        }

        /// <summary>
        /// Gets the logical folder name for the supplied dataset file kind.
        /// </summary>
        public static string GetStorageFolderName(DatasetVersionFileKind fileKind)
        {
            return fileKind == DatasetVersionFileKind.Processed ? "processed" : "raw";
        }
    }
}
