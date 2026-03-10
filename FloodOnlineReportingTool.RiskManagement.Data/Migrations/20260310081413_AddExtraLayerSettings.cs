using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraLayerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SecureLayer",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                keyColumn: "Id",
                keyValue: "parish",
                columns: new[] { "Enabled", "SecureLayer" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers",
                keyColumn: "Id",
                keyValue: "ward",
                columns: new[] { "Enabled", "SecureLayer" },
                values: new object[] { false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers");

            migrationBuilder.DropColumn(
                name: "SecureLayer",
                schema: "fortriskmanagement",
                table: "Lk_DownstreamApiLayers");
        }
    }
}
