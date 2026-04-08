using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetSRIDOfGeomColumnsAndRenameCausesToSource : Migration
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

            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geom",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "geometry(multipolygon, 27700)",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FloodSources",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                newName: "FloodSecondaryCauses");

            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geom",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry(multipolygon, 27700)",
                oldNullable: true);

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
