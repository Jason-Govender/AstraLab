using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddMlExecutionOrchestrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                table: "MLModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlgorithmKey",
                table: "MLExperiments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "legacy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "MLExperiments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispatchErrorMessage",
                table: "MLExperiments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "MLExperiments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskType",
                table: "MLExperiments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                table: "MLExperiments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MLExperiments_TenantId_DatasetVersionId_Status",
                table: "MLExperiments",
                columns: new[] { "TenantId", "DatasetVersionId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MLExperiments_TenantId_DatasetVersionId_Status",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                table: "MLModels");

            migrationBuilder.DropColumn(
                name: "AlgorithmKey",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "DispatchErrorMessage",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "MLExperiments");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                table: "MLExperiments");
        }
    }
}
