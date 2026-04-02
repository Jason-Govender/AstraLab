using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsReportingPersistenceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsightRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    DatasetProfileId = table.Column<long>(type: "bigint", nullable: true),
                    MLExperimentId = table.Column<long>(type: "bigint", nullable: true),
                    AIResponseId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    InsightType = table.Column<int>(type: "integer", nullable: false),
                    InsightSourceType = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_InsightRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsightRecords_AIResponses_AIResponseId",
                        column: x => x.AIResponseId,
                        principalTable: "AIResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsightRecords_DatasetProfiles_DatasetProfileId",
                        column: x => x.DatasetProfileId,
                        principalTable: "DatasetProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsightRecords_DatasetVersions_DatasetVersionId",
                        column: x => x.DatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsightRecords_MLExperiments_MLExperimentId",
                        column: x => x.MLExperimentId,
                        principalTable: "MLExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    DatasetProfileId = table.Column<long>(type: "bigint", nullable: true),
                    MLExperimentId = table.Column<long>(type: "bigint", nullable: true),
                    AIResponseId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ReportFormat = table.Column<int>(type: "integer", nullable: false),
                    ReportSourceType = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ReportRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRecords_AIResponses_AIResponseId",
                        column: x => x.AIResponseId,
                        principalTable: "AIResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRecords_DatasetProfiles_DatasetProfileId",
                        column: x => x.DatasetProfileId,
                        principalTable: "DatasetProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRecords_DatasetVersions_DatasetVersionId",
                        column: x => x.DatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportRecords_MLExperiments_MLExperimentId",
                        column: x => x.MLExperimentId,
                        principalTable: "MLExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsExports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    MLExperimentId = table.Column<long>(type: "bigint", nullable: true),
                    InsightRecordId = table.Column<long>(type: "bigint", nullable: true),
                    ReportRecordId = table.Column<long>(type: "bigint", nullable: true),
                    ExportType = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AnalyticsExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_DatasetVersions_DatasetVersionId",
                        column: x => x.DatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_InsightRecords_InsightRecordId",
                        column: x => x.InsightRecordId,
                        principalTable: "InsightRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_MLExperiments_MLExperimentId",
                        column: x => x.MLExperimentId,
                        principalTable: "MLExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_ReportRecords_ReportRecordId",
                        column: x => x.ReportRecordId,
                        principalTable: "ReportRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_DatasetVersionId",
                table: "AnalyticsExports",
                column: "DatasetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_InsightRecordId",
                table: "AnalyticsExports",
                column: "InsightRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_MLExperimentId",
                table: "AnalyticsExports",
                column: "MLExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_ReportRecordId",
                table: "AnalyticsExports",
                column: "ReportRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_StorageProvider_StorageKey",
                table: "AnalyticsExports",
                columns: new[] { "StorageProvider", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_TenantId_DatasetVersionId_CreationTime",
                table: "AnalyticsExports",
                columns: new[] { "TenantId", "DatasetVersionId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_TenantId_ExportType_CreationTime",
                table: "AnalyticsExports",
                columns: new[] { "TenantId", "ExportType", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_TenantId_MLExperimentId_CreationTime",
                table: "AnalyticsExports",
                columns: new[] { "TenantId", "MLExperimentId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_AIResponseId",
                table: "InsightRecords",
                column: "AIResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_DatasetProfileId",
                table: "InsightRecords",
                column: "DatasetProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_DatasetVersionId",
                table: "InsightRecords",
                column: "DatasetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_MLExperimentId",
                table: "InsightRecords",
                column: "MLExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_TenantId_DatasetVersionId_CreationTime",
                table: "InsightRecords",
                columns: new[] { "TenantId", "DatasetVersionId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_TenantId_InsightSourceType_InsightType_Creat~",
                table: "InsightRecords",
                columns: new[] { "TenantId", "InsightSourceType", "InsightType", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_InsightRecords_TenantId_MLExperimentId_CreationTime",
                table: "InsightRecords",
                columns: new[] { "TenantId", "MLExperimentId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_AIResponseId",
                table: "ReportRecords",
                column: "AIResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_DatasetProfileId",
                table: "ReportRecords",
                column: "DatasetProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_DatasetVersionId",
                table: "ReportRecords",
                column: "DatasetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_MLExperimentId",
                table: "ReportRecords",
                column: "MLExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_TenantId_DatasetVersionId_CreationTime",
                table: "ReportRecords",
                columns: new[] { "TenantId", "DatasetVersionId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_TenantId_MLExperimentId_CreationTime",
                table: "ReportRecords",
                columns: new[] { "TenantId", "MLExperimentId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_TenantId_ReportSourceType_ReportFormat_Creati~",
                table: "ReportRecords",
                columns: new[] { "TenantId", "ReportSourceType", "ReportFormat", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsExports");

            migrationBuilder.DropTable(
                name: "InsightRecords");

            migrationBuilder.DropTable(
                name: "ReportRecords");
        }
    }
}
