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

                entity.HasIndex(dataset => new { dataset.TenantId, dataset.Name });
                entity.HasIndex(dataset => new { dataset.TenantId, dataset.OwnerUserId });
            });
        }
    }
}
