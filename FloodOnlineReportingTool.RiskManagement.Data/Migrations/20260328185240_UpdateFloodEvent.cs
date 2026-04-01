using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFloodEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedNoPhone",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedVulnerable",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedUninhabitable",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedTotalInternallyFlooded",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedTotalExternalOnly",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EstimatedVulnerable",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedUninhabitable",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedTotalInternallyFlooded",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedTotalExternalOnly",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedNoPhone",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "integer",
                nullable: true);
        }
    }
}
