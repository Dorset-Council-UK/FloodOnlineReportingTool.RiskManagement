using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "riskmanagement");

            migrationBuilder.CreateTable(
                name: "FloodEvents",
                schema: "riskmanagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FloodStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FloodEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FloodStatus = table.Column<string>(type: "text", nullable: true),
                    ResponsibleAuthority = table.Column<string>(type: "text", nullable: true),
                    ResponsibleAuthorityOther = table.Column<string>(type: "text", nullable: true),
                    FloodTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloodEvents",
                schema: "riskmanagement");
        }
    }
}
