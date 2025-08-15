using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fortriskmanagement");

            migrationBuilder.CreateTable(
                name: "FloodEvents",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FloodStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FloodEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FloodStatus = table.Column<string>(type: "text", nullable: true),
                    ResponsibleAuthorityOther = table.Column<string>(type: "text", nullable: true),
                    FloodTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibleAuthorities",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibleAuthorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloodEventResponsibleAuthorities",
                schema: "fortriskmanagement",
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
                        principalSchema: "fortriskmanagement",
                        principalTable: "FloodEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloodEventResponsibleAuthorities_ResponsibleAuthorities_Res~",
                        column: x => x.ResponsibleAuthorityId,
                        principalSchema: "fortriskmanagement",
                        principalTable: "ResponsibleAuthorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodEventResponsibleAuthorities_ResponsibleAuthorityId",
                schema: "fortriskmanagement",
                table: "FloodEventResponsibleAuthorities",
                column: "ResponsibleAuthorityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloodEventResponsibleAuthorities",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "FloodEvents",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "ResponsibleAuthorities",
                schema: "fortriskmanagement");
        }
    }
}
