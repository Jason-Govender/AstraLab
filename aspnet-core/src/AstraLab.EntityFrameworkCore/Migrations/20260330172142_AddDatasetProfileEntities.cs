using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasetProfileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatasetProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetVersionId = table.Column<long>(type: "bigint", nullable: false),
                    RowCount = table.Column<long>(type: "bigint", nullable: false),
                    DuplicateRowCount = table.Column<long>(type: "bigint", nullable: false),
                    DataHealthScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_DatasetProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatasetProfiles_DatasetVersions_DatasetVersionId",
                        column: x => x.DatasetVersionId,
                        principalTable: "DatasetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatasetColumnProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    DatasetProfileId = table.Column<long>(type: "bigint", nullable: false),
                    DatasetColumnId = table.Column<long>(type: "bigint", nullable: false),
                    InferredDataType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NullCount = table.Column<long>(type: "bigint", nullable: false),
                    DistinctCount = table.Column<long>(type: "bigint", nullable: true),
                    StatisticsJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DatasetColumnProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatasetColumnProfiles_DatasetColumns_DatasetColumnId",
                        column: x => x.DatasetColumnId,
                        principalTable: "DatasetColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatasetColumnProfiles_DatasetProfiles_DatasetProfileId",
                        column: x => x.DatasetProfileId,
                        principalTable: "DatasetProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatasetColumnProfiles_DatasetColumnId",
                table: "DatasetColumnProfiles",
                column: "DatasetColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_DatasetColumnProfiles_DatasetProfileId_DatasetColumnId",
                table: "DatasetColumnProfiles",
                columns: new[] { "DatasetProfileId", "DatasetColumnId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatasetColumnProfiles_TenantId_DatasetProfileId",
                table: "DatasetColumnProfiles",
                columns: new[] { "TenantId", "DatasetProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_DatasetProfiles_DatasetVersionId",
                table: "DatasetProfiles",
                column: "DatasetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatasetProfiles_TenantId_DatasetVersionId",
                table: "DatasetProfiles",
                columns: new[] { "TenantId", "DatasetVersionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatasetColumnProfiles");

            migrationBuilder.DropTable(
                name: "DatasetProfiles");
        }
    }
}
