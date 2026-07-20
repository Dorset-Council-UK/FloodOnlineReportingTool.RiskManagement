using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipMediaToFloodEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaItems_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "MediaItems");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaItems_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "MediaItems",
                column: "FloodEventId",
                principalSchema: "fortriskmanagement",
                principalTable: "FloodEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaItems_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "MediaItems");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaItems_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "MediaItems",
                column: "FloodEventId",
                principalSchema: "fortriskmanagement",
                principalTable: "FloodEvents",
                principalColumn: "Id");
        }
    }
}
