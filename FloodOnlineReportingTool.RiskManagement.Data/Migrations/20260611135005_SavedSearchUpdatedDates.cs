using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SavedSearchUpdatedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "fortriskmanagement",
                table: "SavedSearches");
        }
    }
}
