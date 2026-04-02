using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Core.Domains.Datasets
{
    /// <summary>
    /// Represents the current persisted profiling snapshot for a dataset version.
    /// </summary>
    public class DatasetProfile : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The maximum allowed length for the summary payload.
        /// </summary>
        public const string SummaryJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the dataset profile.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that owns the profile snapshot.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that owns the profile snapshot.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the profiled row count for the dataset version.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled duplicate row count for the dataset version.
        /// </summary>
        public long DuplicateRowCount { get; set; }

        /// <summary>
        /// Gets or sets the profiled data health score for the dataset version.
        /// </summary>
        public decimal DataHealthScore { get; set; }

        /// <summary>
        /// Gets or sets the serialized profile summary payload when extra profile metadata is available.
        /// </summary>
        public string SummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the persisted column-level profile rows for this profiling snapshot.
        /// </summary>
        public ICollection<DatasetColumnProfile> ColumnProfiles { get; set; } = new List<DatasetColumnProfile>();

        /// <summary>
        /// Gets or sets the persisted analytics insights informed by this profile.
        /// </summary>
        public ICollection<InsightRecord> InsightRecords { get; set; } = new List<InsightRecord>();

        /// <summary>
        /// Gets or sets the persisted reports informed by this profile.
        /// </summary>
        public ICollection<ReportRecord> ReportRecords { get; set; } = new List<ReportRecord>();
    }
}
