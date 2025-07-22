using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFloodEventResponsibleAuthority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodEvents_ResponsibleAuthorities_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                table: "FloodEvents");

            migrationBuilder.CreateTable(
                name: "FloodEventResponsibleAuthorities",
                schema: "riskmanagement",
                columns: table => new
                {
                    FloodEventId = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleAuthorityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEventResponsibleAuthorities", x => new { x.FloodEventId, x.ResponsibleAuthorityId });
                    table.ForeignKey(
                        name: "FK_FloodEventResponsibleAuthorities_FloodEvents_FloodEventId",
                        column: x => x.FloodEventId,
                        principalSchema: "riskmanagement",
                        principalTable: "FloodEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloodEventResponsibleAuthorities_ResponsibleAuthorities_Res~",
                        column: x => x.ResponsibleAuthorityId,
                        principalSchema: "riskmanagement",
                        principalTable: "ResponsibleAuthorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodEventResponsibleAuthorities_ResponsibleAuthorityId",
                schema: "riskmanagement",
                table: "FloodEventResponsibleAuthorities",
                column: "ResponsibleAuthorityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloodEventResponsibleAuthorities",
                schema: "riskmanagement");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleAuthoritesId",
                schema: "riskmanagement",
                table: "FloodEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleAuthority",
                schema: "riskmanagement",
                table: "FloodEvents",
                type: "uuid",
                nullable: true);

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
    }
}
