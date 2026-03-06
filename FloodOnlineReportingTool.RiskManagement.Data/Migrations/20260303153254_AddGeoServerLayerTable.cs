using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoServerLayerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownstreamApiLayers",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiSource = table.Column<string>(type: "text", nullable: false),
                    LayerName = table.Column<string>(type: "text", nullable: false),
                    FeatureIdentifier = table.Column<string>(type: "text", nullable: false),
                    RequestMethod = table.Column<int>(type: "integer", nullable: false),
                    RequestVersion = table.Column<int>(type: "integer", nullable: false),
                    OrderBy = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownstreamApiLayers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownstreamApiLayers",
                schema: "fortriskmanagement");
        }
    }
}
