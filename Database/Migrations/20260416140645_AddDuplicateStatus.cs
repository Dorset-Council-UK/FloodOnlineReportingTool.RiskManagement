using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_ReportStatuses",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[] { new Guid("018feb13-0b00-7c8e-9a1c-1d9b2f8c3c44"), "A potential duplicate has been found for this record. This needs to be reviewed.", 5, "Duplicate Detected" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "fortriskmanagement",
                table: "Lk_ReportStatuses",
                keyColumn: "Id",
                keyValue: new Guid("018feb13-0b00-7c8e-9a1c-1d9b2f8c3c44"));
        }
    }
}
