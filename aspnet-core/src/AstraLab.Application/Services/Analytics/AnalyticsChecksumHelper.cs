using System.Security.Cryptography;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Provides checksum helpers for exported analytics payloads.
    /// </summary>
    public static class AnalyticsChecksumHelper
    {
        /// <summary>
        /// Computes a SHA-256 checksum as a lowercase hexadecimal string.
        /// </summary>
        public static string ComputeSha256(byte[] content)
        {
            if (content == null)
            {
                return null;
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(content);
                return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
