using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AstraLab.Authorization.Roles;
using AstraLab.Authorization.Users;
using AstraLab.Core.Domains.Datasets;
using AstraLab.MultiTenancy;

namespace AstraLab.EntityFrameworkCore
{
    public class AstraLabDbContext : AbpZeroDbContext<Tenant, Role, User, AstraLabDbContext>
    {
        /* Define a DbSet for each entity of the application */

        /// <summary>
        /// Gets or sets the persisted datasets for the ingestion domain.
        /// </summary>
        public DbSet<Dataset> Datasets { get; set; }

        /// <summary>
        /// Gets or sets the persisted dataset versions for ingestion lineage.
        /// </summary>
        public DbSet<DatasetVersion> DatasetVersions { get; set; }

        /// <summary>
        /// Gets or sets the persisted dataset columns for structural metadata.
        /// </summary>
        public DbSet<DatasetColumn> DatasetColumns { get; set; }

        /// <summary>
        /// Gets or sets the persisted raw file references for dataset versions.
        /// </summary>
        public DbSet<DatasetFile> DatasetFiles { get; set; }

        /// <summary>
        /// Gets or sets the persisted dataset-level profiling snapshots.
        /// </summary>
        public DbSet<DatasetProfile> DatasetProfiles { get; set; }

        /// <summary>
        /// Gets or sets the persisted column-level profiling snapshots.
        /// </summary>
        public DbSet<DatasetColumnProfile> DatasetColumnProfiles { get; set; }

        public AstraLabDbContext(DbContextOptions<AstraLabDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("Datasets");

                entity.Property(dataset => dataset.Name)
                    .IsRequired()
                    .HasMaxLength(Dataset.MaxNameLength);

                entity.Property(dataset => dataset.Description)
                    .HasMaxLength(Dataset.MaxDescriptionLength);

                entity.Property(dataset => dataset.OriginalFileName)
                    .IsRequired()
                    .HasMaxLength(Dataset.MaxOriginalFileNameLength);

                entity.HasOne(dataset => dataset.CurrentVersion)
                    .WithMany()
                    .HasForeignKey(dataset => dataset.CurrentVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(dataset => new { dataset.TenantId, dataset.Name });
                entity.HasIndex(dataset => new { dataset.TenantId, dataset.OwnerUserId });
                entity.HasIndex(dataset => dataset.CurrentVersionId);
            });

            modelBuilder.Entity<DatasetVersion>(entity =>
            {
                entity.ToTable("DatasetVersions");

                entity.Property(datasetVersion => datasetVersion.SchemaJson)
                    .HasColumnType("text");

                entity.HasOne(datasetVersion => datasetVersion.Dataset)
                    .WithMany(dataset => dataset.Versions)
                    .HasForeignKey(datasetVersion => datasetVersion.DatasetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(datasetVersion => datasetVersion.ParentVersion)
                    .WithMany()
                    .HasForeignKey(datasetVersion => datasetVersion.ParentVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(datasetVersion => new { datasetVersion.DatasetId, datasetVersion.VersionNumber })
                    .IsUnique();

                entity.HasIndex(datasetVersion => new { datasetVersion.TenantId, datasetVersion.DatasetId });
                entity.HasIndex(datasetVersion => datasetVersion.ParentVersionId);
            });

            modelBuilder.Entity<DatasetColumn>(entity =>
            {
                entity.ToTable("DatasetColumns");

                entity.Property(datasetColumn => datasetColumn.Name)
                    .IsRequired()
                    .HasMaxLength(DatasetColumn.MaxNameLength);

                entity.Property(datasetColumn => datasetColumn.DataType)
                    .IsRequired()
                    .HasMaxLength(DatasetColumn.MaxDataTypeLength);

                entity.HasOne(datasetColumn => datasetColumn.DatasetVersion)
                    .WithMany(datasetVersion => datasetVersion.Columns)
                    .HasForeignKey(datasetColumn => datasetColumn.DatasetVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(datasetColumn => new { datasetColumn.DatasetVersionId, datasetColumn.Ordinal })
                    .IsUnique();

                entity.HasIndex(datasetColumn => new { datasetColumn.TenantId, datasetColumn.DatasetVersionId });
                entity.HasIndex(datasetColumn => new { datasetColumn.DatasetVersionId, datasetColumn.Name });
            });

            modelBuilder.Entity<DatasetFile>(entity =>
            {
                entity.ToTable("DatasetFiles");

                entity.Property(datasetFile => datasetFile.StorageProvider)
                    .IsRequired()
                    .HasMaxLength(DatasetFile.MaxStorageProviderLength);

                entity.Property(datasetFile => datasetFile.StorageKey)
                    .IsRequired()
                    .HasMaxLength(DatasetFile.MaxStorageKeyLength);

                entity.Property(datasetFile => datasetFile.OriginalFileName)
                    .IsRequired()
                    .HasMaxLength(DatasetFile.MaxOriginalFileNameLength);

                entity.Property(datasetFile => datasetFile.ContentType)
                    .HasMaxLength(DatasetFile.MaxContentTypeLength);

                entity.Property(datasetFile => datasetFile.ChecksumSha256)
                    .IsRequired()
                    .HasMaxLength(DatasetFile.ChecksumSha256Length);

                entity.HasOne(datasetFile => datasetFile.DatasetVersion)
                    .WithOne(datasetVersion => datasetVersion.RawFile)
                    .HasForeignKey<DatasetFile>(datasetFile => datasetFile.DatasetVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(datasetFile => datasetFile.DatasetVersionId)
                    .IsUnique();

                entity.HasIndex(datasetFile => new { datasetFile.StorageProvider, datasetFile.StorageKey })
                    .IsUnique();
            });

            modelBuilder.Entity<DatasetProfile>(entity =>
            {
                entity.ToTable("DatasetProfiles");

                entity.Property(datasetProfile => datasetProfile.DataHealthScore)
                    .HasPrecision(5, 2);

                entity.Property(datasetProfile => datasetProfile.SummaryJson)
                    .HasColumnType(DatasetProfile.SummaryJsonColumnType);

                entity.HasOne(datasetProfile => datasetProfile.DatasetVersion)
                    .WithOne(datasetVersion => datasetVersion.Profile)
                    .HasForeignKey<DatasetProfile>(datasetProfile => datasetProfile.DatasetVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(datasetProfile => datasetProfile.DatasetVersionId)
                    .IsUnique();

                entity.HasIndex(datasetProfile => new { datasetProfile.TenantId, datasetProfile.DatasetVersionId });
            });

            modelBuilder.Entity<DatasetColumnProfile>(entity =>
            {
                entity.ToTable("DatasetColumnProfiles");

                entity.Property(datasetColumnProfile => datasetColumnProfile.InferredDataType)
                    .IsRequired()
                    .HasMaxLength(DatasetColumnProfile.MaxInferredDataTypeLength);

                entity.Property(datasetColumnProfile => datasetColumnProfile.StatisticsJson)
                    .HasColumnType(DatasetColumnProfile.StatisticsJsonColumnType);

                entity.HasOne(datasetColumnProfile => datasetColumnProfile.DatasetProfile)
                    .WithMany(datasetProfile => datasetProfile.ColumnProfiles)
                    .HasForeignKey(datasetColumnProfile => datasetColumnProfile.DatasetProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(datasetColumnProfile => datasetColumnProfile.DatasetColumn)
                    .WithMany(datasetColumn => datasetColumn.Profiles)
                    .HasForeignKey(datasetColumnProfile => datasetColumnProfile.DatasetColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(datasetColumnProfile => new { datasetColumnProfile.DatasetProfileId, datasetColumnProfile.DatasetColumnId })
                    .IsUnique();

                entity.HasIndex(datasetColumnProfile => new { datasetColumnProfile.TenantId, datasetColumnProfile.DatasetProfileId });
                entity.HasIndex(datasetColumnProfile => datasetColumnProfile.DatasetColumnId);
            });
        }
    }
}
