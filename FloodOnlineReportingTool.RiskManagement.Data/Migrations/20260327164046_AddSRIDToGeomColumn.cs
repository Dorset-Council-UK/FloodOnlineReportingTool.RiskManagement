using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSRIDToGeomColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geom",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry(multipolygon, 27700)",
                oldNullable: true);
        }
    }
}
