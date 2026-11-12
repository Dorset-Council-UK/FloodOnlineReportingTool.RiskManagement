using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicMediaSubmissionsMetaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PublicMediaSubmissions",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmailAddress = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SharingAgreementAccepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicMediaSubmissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems",
                column: "PublicMediaSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicMediaSubmissions_Reference",
                schema: "fortriskmanagement",
                table: "PublicMediaSubmissions",
                column: "Reference",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaItems_PublicMediaSubmissions_PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems",
                column: "PublicMediaSubmissionId",
                principalSchema: "fortriskmanagement",
                principalTable: "PublicMediaSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaItems_PublicMediaSubmissions_PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems");

            migrationBuilder.DropTable(
                name: "PublicMediaSubmissions",
                schema: "fortriskmanagement");

            migrationBuilder.DropIndex(
                name: "IX_MediaItems_PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems");

            migrationBuilder.DropColumn(
                name: "PublicMediaSubmissionId",
                schema: "fortriskmanagement",
                table: "MediaItems");
        }
    }
}
