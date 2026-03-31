using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasetTransformationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatasetTransformations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    SourceDatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ResultDatasetVersionId = table.Column<long>(type: "bigint", nullable: true),
                    TransformationType = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SummaryJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DatasetTransformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatasetTransformations_DatasetVersions_ResultDatasetVersion~",
                        column: x => x.ResultDatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatasetTransformations_DatasetVersions_SourceDatasetVersion~",
                        column: x => x.SourceDatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatasetTransformations_ResultDatasetVersionId",
                table: "DatasetTransformations",
                column: "ResultDatasetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatasetTransformations_SourceDatasetVersionId_ExecutionOrder",
                table: "DatasetTransformations",
                columns: new[] { "SourceDatasetVersionId", "ExecutionOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatasetTransformations_TenantId_ExecutedAt",
                table: "DatasetTransformations",
                columns: new[] { "TenantId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DatasetTransformations_TenantId_SourceDatasetVersionId",
                table: "DatasetTransformations",
                columns: new[] { "TenantId", "SourceDatasetVersionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatasetTransformations");
        }
    }
}
