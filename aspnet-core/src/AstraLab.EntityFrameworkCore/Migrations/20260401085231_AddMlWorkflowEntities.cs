using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddMlWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MLExperiments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    TargetDatasetColumnId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TrainingConfigurationJson = table.Column<string>(type: "text", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailureMessage = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLExperiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MLExperiments_DatasetColumns_TargetDatasetColumnId",
                        column: x => x.TargetDatasetColumnId,
                        principalTable: "DatasetColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MLExperiments_DatasetVersions_DatasetVersionId",
                        column: x => x.DatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLExperimentFeatures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    MLExperimentId = table.Column<long>(type: "bigint", nullable: false),
                    DatasetColumnId = table.Column<long>(type: "bigint", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLExperimentFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MLExperimentFeatures_DatasetColumns_DatasetColumnId",
                        column: x => x.DatasetColumnId,
                        principalTable: "DatasetColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MLExperimentFeatures_MLExperiments_MLExperimentId",
                        column: x => x.MLExperimentId,
                        principalTable: "MLExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLModels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    MLExperimentId = table.Column<long>(type: "bigint", nullable: false),
                    ModelType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArtifactStorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ArtifactStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformanceSummaryJson = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MLModels_MLExperiments_MLExperimentId",
                        column: x => x.MLExperimentId,
                        principalTable: "MLExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLModelFeatureImportances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    MLModelId = table.Column<long>(type: "bigint", nullable: false),
                    DatasetColumnId = table.Column<long>(type: "bigint", nullable: false),
                    ImportanceScore = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModelFeatureImportances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MLModelFeatureImportances_DatasetColumns_DatasetColumnId",
                        column: x => x.DatasetColumnId,
                        principalTable: "DatasetColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MLModelFeatureImportances_MLModels_MLModelId",
                        column: x => x.MLModelId,
                        principalTable: "MLModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLModelMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    MLModelId = table.Column<long>(type: "bigint", nullable: false),
                    MetricName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MetricValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModelMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MLModelMetrics_MLModels_MLModelId",
                        column: x => x.MLModelId,
                        principalTable: "MLModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MLExperimentFeatures_DatasetColumnId",
                table: "MLExperimentFeatures",
                column: "DatasetColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_MLExperimentFeatures_MLExperimentId_DatasetColumnId",
                table: "MLExperimentFeatures",
                columns: new[] { "MLExperimentId", "DatasetColumnId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLExperimentFeatures_MLExperimentId_Ordinal",
                table: "MLExperimentFeatures",
                columns: new[] { "MLExperimentId", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLExperiments_DatasetVersionId",
                table: "MLExperiments",
                column: "DatasetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MLExperiments_TargetDatasetColumnId",
                table: "MLExperiments",
                column: "TargetDatasetColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_MLExperiments_TenantId_DatasetVersionId_ExecutedAt",
                table: "MLExperiments",
                columns: new[] { "TenantId", "DatasetVersionId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MLModelFeatureImportances_DatasetColumnId",
                table: "MLModelFeatureImportances",
                column: "DatasetColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_MLModelFeatureImportances_MLModelId_DatasetColumnId",
                table: "MLModelFeatureImportances",
                columns: new[] { "MLModelId", "DatasetColumnId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLModelMetrics_MLModelId_MetricName",
                table: "MLModelMetrics",
                columns: new[] { "MLModelId", "MetricName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLModels_ArtifactStorageProvider_ArtifactStorageKey",
                table: "MLModels",
                columns: new[] { "ArtifactStorageProvider", "ArtifactStorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLModels_MLExperimentId",
                table: "MLModels",
                column: "MLExperimentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MLExperimentFeatures");

            migrationBuilder.DropTable(
                name: "MLModelFeatureImportances");

            migrationBuilder.DropTable(
                name: "MLModelMetrics");

            migrationBuilder.DropTable(
                name: "MLModels");

            migrationBuilder.DropTable(
                name: "MLExperiments");
        }
    }
}
