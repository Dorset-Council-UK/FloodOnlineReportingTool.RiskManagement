using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixGeometryTypeDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "PolygonGeom",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "geometry(multipolygon, 27700)",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<Point>(
                name: "PointGeom",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "geometry(point, 27700)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "PolygonGeom",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry(multipolygon, 27700)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Point>(
                name: "PointGeom",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry(point, 27700)",
                oldNullable: true);
        }
    }
}
