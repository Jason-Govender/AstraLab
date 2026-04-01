using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AstraLab.Authorization.Roles;
using AstraLab.Authorization.Users;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
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

        /// <summary>
        /// Gets or sets the persisted transformation history rows for dataset version processing.
        /// </summary>
        public DbSet<DatasetTransformation> DatasetTransformations { get; set; }

        /// <summary>
        /// Gets or sets the persisted machine learning experiment runs.
        /// </summary>
        public DbSet<MLExperiment> MLExperiments { get; set; }

        /// <summary>
        /// Gets or sets the persisted trained model metadata rows.
        /// </summary>
        public DbSet<MLModel> MLModels { get; set; }

        /// <summary>
        /// Gets or sets the persisted experiment feature selection rows.
        /// </summary>
        public DbSet<MLExperimentFeature> MLExperimentFeatures { get; set; }

        /// <summary>
        /// Gets or sets the persisted model metric rows.
        /// </summary>
        public DbSet<MLModelMetric> MLModelMetrics { get; set; }

        /// <summary>
        /// Gets or sets the persisted model feature importance rows.
        /// </summary>
        public DbSet<MLModelFeatureImportance> MLModelFeatureImportances { get; set; }

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

            modelBuilder.Entity<DatasetTransformation>(entity =>
            {
                entity.ToTable("DatasetTransformations");

                entity.Property(datasetTransformation => datasetTransformation.ConfigurationJson)
                    .IsRequired()
                    .HasColumnType(DatasetTransformation.ConfigurationJsonColumnType);

                entity.Property(datasetTransformation => datasetTransformation.SummaryJson)
                    .HasColumnType(DatasetTransformation.SummaryJsonColumnType);

                entity.HasOne(datasetTransformation => datasetTransformation.SourceDatasetVersion)
                    .WithMany(datasetVersion => datasetVersion.OutgoingTransformations)
                    .HasForeignKey(datasetTransformation => datasetTransformation.SourceDatasetVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(datasetTransformation => datasetTransformation.ResultDatasetVersion)
                    .WithOne(datasetVersion => datasetVersion.ProducedByTransformation)
                    .HasForeignKey<DatasetTransformation>(datasetTransformation => datasetTransformation.ResultDatasetVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(datasetTransformation => new { datasetTransformation.TenantId, datasetTransformation.SourceDatasetVersionId });

                entity.HasIndex(datasetTransformation => datasetTransformation.ResultDatasetVersionId)
                    .IsUnique();

                entity.HasIndex(datasetTransformation => new { datasetTransformation.SourceDatasetVersionId, datasetTransformation.ExecutionOrder })
                    .IsUnique();

                entity.HasIndex(datasetTransformation => new { datasetTransformation.TenantId, datasetTransformation.ExecutedAt });
            });

            modelBuilder.Entity<MLExperiment>(entity =>
            {
                entity.ToTable("MLExperiments");

                entity.Property(mlExperiment => mlExperiment.TrainingConfigurationJson)
                    .IsRequired()
                    .HasColumnType(MLExperiment.TrainingConfigurationJsonColumnType);

                entity.Property(mlExperiment => mlExperiment.FailureMessage)
                    .HasColumnType(MLExperiment.FailureMessageColumnType);

                entity.HasOne(mlExperiment => mlExperiment.DatasetVersion)
                    .WithMany(datasetVersion => datasetVersion.MlExperiments)
                    .HasForeignKey(mlExperiment => mlExperiment.DatasetVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mlExperiment => mlExperiment.TargetDatasetColumn)
                    .WithMany()
                    .HasForeignKey(mlExperiment => mlExperiment.TargetDatasetColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(mlExperiment => new { mlExperiment.TenantId, mlExperiment.DatasetVersionId, mlExperiment.ExecutedAt });
                entity.HasIndex(mlExperiment => mlExperiment.TargetDatasetColumnId);
            });

            modelBuilder.Entity<MLModel>(entity =>
            {
                entity.ToTable("MLModels");

                entity.Property(mlModel => mlModel.ModelType)
                    .IsRequired()
                    .HasMaxLength(MLModel.MaxModelTypeLength);

                entity.Property(mlModel => mlModel.ArtifactStorageProvider)
                    .HasMaxLength(DatasetFile.MaxStorageProviderLength);

                entity.Property(mlModel => mlModel.ArtifactStorageKey)
                    .HasMaxLength(DatasetFile.MaxStorageKeyLength);

                entity.Property(mlModel => mlModel.PerformanceSummaryJson)
                    .HasColumnType(MLModel.PerformanceSummaryJsonColumnType);

                entity.HasOne(mlModel => mlModel.MLExperiment)
                    .WithOne(mlExperiment => mlExperiment.Model)
                    .HasForeignKey<MLModel>(mlModel => mlModel.MLExperimentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(mlModel => mlModel.MLExperimentId)
                    .IsUnique();

                entity.HasIndex(mlModel => new { mlModel.ArtifactStorageProvider, mlModel.ArtifactStorageKey })
                    .IsUnique();
            });

            modelBuilder.Entity<MLExperimentFeature>(entity =>
            {
                entity.ToTable("MLExperimentFeatures");

                entity.HasOne(mlExperimentFeature => mlExperimentFeature.MLExperiment)
                    .WithMany(mlExperiment => mlExperiment.SelectedFeatures)
                    .HasForeignKey(mlExperimentFeature => mlExperimentFeature.MLExperimentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mlExperimentFeature => mlExperimentFeature.DatasetColumn)
                    .WithMany()
                    .HasForeignKey(mlExperimentFeature => mlExperimentFeature.DatasetColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(mlExperimentFeature => new { mlExperimentFeature.MLExperimentId, mlExperimentFeature.DatasetColumnId })
                    .IsUnique();

                entity.HasIndex(mlExperimentFeature => new { mlExperimentFeature.MLExperimentId, mlExperimentFeature.Ordinal })
                    .IsUnique();
            });

            modelBuilder.Entity<MLModelMetric>(entity =>
            {
                entity.ToTable("MLModelMetrics");

                entity.Property(mlModelMetric => mlModelMetric.MetricName)
                    .IsRequired()
                    .HasMaxLength(MLModelMetric.MaxMetricNameLength);

                entity.Property(mlModelMetric => mlModelMetric.MetricValue)
                    .HasPrecision(18, 6);

                entity.HasOne(mlModelMetric => mlModelMetric.MLModel)
                    .WithMany(mlModel => mlModel.Metrics)
                    .HasForeignKey(mlModelMetric => mlModelMetric.MLModelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(mlModelMetric => new { mlModelMetric.MLModelId, mlModelMetric.MetricName })
                    .IsUnique();
            });

            modelBuilder.Entity<MLModelFeatureImportance>(entity =>
            {
                entity.ToTable("MLModelFeatureImportances");

                entity.Property(mlModelFeatureImportance => mlModelFeatureImportance.ImportanceScore)
                    .HasPrecision(18, 6);

                entity.HasOne(mlModelFeatureImportance => mlModelFeatureImportance.MLModel)
                    .WithMany(mlModel => mlModel.FeatureImportances)
                    .HasForeignKey(mlModelFeatureImportance => mlModelFeatureImportance.MLModelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mlModelFeatureImportance => mlModelFeatureImportance.DatasetColumn)
                    .WithMany()
                    .HasForeignKey(mlModelFeatureImportance => mlModelFeatureImportance.DatasetColumnId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(mlModelFeatureImportance => new { mlModelFeatureImportance.MLModelId, mlModelFeatureImportance.DatasetColumnId })
                    .IsUnique();
            });
        }
    }
}
