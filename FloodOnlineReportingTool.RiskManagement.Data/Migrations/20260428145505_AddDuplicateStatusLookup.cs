using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateStatusLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "fortriskmanagement",
                table: "Lk_ReportStatuses",
                keyColumn: "Id",
                keyValue: new Guid("018feb13-e260-7c11-9106-c179ba7c8ce4"),
                column: "Order",
                value: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "fortriskmanagement",
                table: "Lk_ReportStatuses",
                keyColumn: "Id",
                keyValue: new Guid("018feb13-e260-7c11-9106-c179ba7c8ce4"),
                column: "Order",
                value: 5);
        }
    }
}
