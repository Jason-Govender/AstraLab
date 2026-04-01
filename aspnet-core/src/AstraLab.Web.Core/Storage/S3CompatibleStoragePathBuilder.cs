using AstraLab.Services.Storage;

namespace AstraLab.Web.Core.Storage
{
    /// <summary>
    /// Builds normalized object keys beneath configured S3-compatible storage prefixes.
    /// </summary>
    public static class S3CompatibleStoragePathBuilder
    {
        /// <summary>
        /// Builds the full object key for a dataset file.
        /// </summary>
        public static string BuildDatasetObjectKey(ObjectStorageOptions options, string storageKey)
        {
            return Combine(options.DatasetKeyPrefix, storageKey);
        }

        /// <summary>
        /// Builds the full object key for an ML artifact file.
        /// </summary>
        public static string BuildMlArtifactObjectKey(ObjectStorageOptions options, string storageKey)
        {
            return Combine(options.MlArtifactKeyPrefix, storageKey);
        }

        private static string Combine(string prefix, string storageKey)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
                ? string.Empty
                : prefix.Trim().Trim('/');

            var normalizedStorageKey = storageKey.Trim().TrimStart('/');
            return string.IsNullOrWhiteSpace(normalizedPrefix)
                ? normalizedStorageKey
                : normalizedPrefix + "/" + normalizedStorageKey;
        }
    }
}
