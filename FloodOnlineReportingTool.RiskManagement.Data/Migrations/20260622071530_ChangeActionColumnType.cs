using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeActionColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionedByUserId",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.AddColumn<string>(
                name: "ActionedBy",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionedBy",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "ActionedByUserId",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "uuid",
                nullable: true);
        }
    }
}
