using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameResponsibleAuthorityColumnToResponsibleAuthorities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthority_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponsibleAuthority",
                schema: "riskmanagement",
                table: "ResponsibleAuthority");

            migrationBuilder.RenameTable(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                newName: "ResponsibleAuthorities",
                newSchema: "riskmanagement");

            migrationBuilder.RenameColumn(
                name: "ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEvents",
                newName: "ResponsibleAuthority");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponsibleAuthorities",
                schema: "riskmanagement",
                table: "ResponsibleAuthorities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FloodEvents_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents",
                column: "ResponsibleAuthoritesId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthorities_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents",
                column: "ResponsibleAuthoritesId",
                principalSchema: "riskmanagement",
                principalTable: "ResponsibleAuthorities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthorities_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponsibleAuthorities",
                schema: "riskmanagement",
                table: "ResponsibleAuthorities");

            migrationBuilder.DropColumn(
                name: "ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.RenameTable(
                name: "ResponsibleAuthorities",
                schema: "riskmanagement",
                newName: "ResponsibleAuthority",
                newSchema: "riskmanagement");

            migrationBuilder.RenameColumn(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                table: "FloodEvents",
                newName: "ResponsibleAuthorityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponsibleAuthority",
                schema: "riskmanagement",
                table: "ResponsibleAuthority",
                column: "Id");

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
    }
}
