using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstraLab.Migrations
{
    /// <inheritdoc />
    public partial class AddAiResponseMlExperimentLinkage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MLExperimentId",
                table: "AIResponses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIResponses_MLExperimentId",
                table: "AIResponses",
                column: "MLExperimentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIResponses_MLExperiments_MLExperimentId",
                table: "AIResponses",
                column: "MLExperimentId",
                principalTable: "MLExperiments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIResponses_MLExperiments_MLExperimentId",
                table: "AIResponses");

            migrationBuilder.DropIndex(
                name: "IX_AIResponses_MLExperimentId",
                table: "AIResponses");

            migrationBuilder.DropColumn(
                name: "MLExperimentId",
                table: "AIResponses");
        }
    }
}
