using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasetVersionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CurrentVersionId",
                table: "Datasets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DatasetVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    VersionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ParentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    RowCount = table.Column<int>(type: "integer", nullable: true),
                    ColumnCount = table.Column<int>(type: "integer", nullable: true),
                    SchemaJson = table.Column<string>(type: "text", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_DatasetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatasetVersions_DatasetVersions_ParentVersionId",
                        column: x => x.ParentVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatasetVersions_Datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "Datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Datasets_CurrentVersionId",
                table: "Datasets",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DatasetVersions_DatasetId_VersionNumber",
                table: "DatasetVersions",
                columns: new[] { "DatasetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatasetVersions_ParentVersionId",
                table: "DatasetVersions",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DatasetVersions_TenantId_DatasetId",
                table: "DatasetVersions",
                columns: new[] { "TenantId", "DatasetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Datasets_DatasetVersions_CurrentVersionId",
                table: "Datasets",
                column: "CurrentVersionId",
                principalTable: "DatasetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Datasets_DatasetVersions_CurrentVersionId",
                table: "Datasets");

            migrationBuilder.DropTable(
                name: "DatasetVersions");

            migrationBuilder.DropIndex(
                name: "IX_Datasets_CurrentVersionId",
                table: "Datasets");

            migrationBuilder.DropColumn(
                name: "CurrentVersionId",
                table: "Datasets");
        }
    }
}
