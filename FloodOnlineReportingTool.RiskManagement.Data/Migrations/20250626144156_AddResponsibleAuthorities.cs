using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibleAuthorities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibleAuthority", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodEvents_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents",
                column: "ResponsibleAuthorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthority_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents",
                column: "ResponsibleAuthorityId",
                principalSchema: "riskmanagement",
                principalTable: "ResponsibleAuthority",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthority_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropTable(
                name: "ResponsibleAuthority",
                schema: "riskmanagement");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                table: "FloodEvents",
                type: "text",
                nullable: true);
        }
    }
}
