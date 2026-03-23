using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCausesToSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FloodPrimaryCauses",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.RenameColumn(
                name: "FloodSecondaryCauses",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                newName: "FloodSources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FloodSources",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                newName: "FloodSecondaryCauses");

            migrationBuilder.AddColumn<Guid[]>(
                name: "FloodPrimaryCauses",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }
    }
}
