using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGeoServerLayerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DownstreamApiLayers",
                schema: "fortriskmanagement",
                table: "DownstreamApiLayers");

            migrationBuilder.RenameTable(
                name: "DownstreamApiLayers",
                schema: "fortriskmanagement",
                newName: "Lk_DownstreamApiLayers",
                newSchema: "fortriskmanagement");

            migrationBuilder.AlterTable(
                name: "Lk_DownstreamApiLayers",
                schema: "fortriskmanagement",
                comment: "All possible downstream API layers and their user friendly descriptions");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ApiPath",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lk_DownstreamApiLayers",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                column: "Id");

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                columns: new[] { "Id", "ApiPath", "ApiSource", "FeatureIdentifier", "LayerName", "OrderBy", "RequestMethod", "RequestVersion", "SortOrder" },
                values: new object[,]
                {
                    { "parish", "/geoserver/ows", "GeoServer", "name", "", "name", 0, 0, 0 },
                    { "ward", "/geoserver/ows", "GeoServer", "name", "", "name", 0, 0, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lk_DownstreamApiLayers",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers");

            migrationBuilder.DeleteData(
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                keyColumn: "Id",
                keyValue: "parish");

            migrationBuilder.DeleteData(
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                keyColumn: "Id",
                keyValue: "ward");

            migrationBuilder.DropColumn(
                name: "ApiPath",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers");

            migrationBuilder.RenameTable(
                name: "Lk_DownstreamApiLayers",
                schema: "fortriskmanagement",
                newName: "DownstreamApiLayers",
                newSchema: "fortriskmanagement");

            migrationBuilder.AlterTable(
                name: "DownstreamApiLayers",
                schema: "fortriskmanagement",
                oldComment: "All possible downstream API layers and their user friendly descriptions");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "fortriskmanagement",
                table: "DownstreamApiLayers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownstreamApiLayers",
                schema: "fortriskmanagement",
                table: "DownstreamApiLayers",
                column: "Id");
        }
    }
}
