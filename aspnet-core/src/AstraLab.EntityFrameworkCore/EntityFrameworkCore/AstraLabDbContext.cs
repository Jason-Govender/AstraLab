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
        }
    }
}
