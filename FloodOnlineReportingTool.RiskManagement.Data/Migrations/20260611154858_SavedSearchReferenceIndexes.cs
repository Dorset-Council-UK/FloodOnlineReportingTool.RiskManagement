using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SavedSearchReferenceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSearches_Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodEvents_Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SavedSearches_Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
