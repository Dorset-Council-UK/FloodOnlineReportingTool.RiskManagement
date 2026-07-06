using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFloodEventReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedSearches_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.DropIndex(
                name: "IX_SavedSearches_FloodEventId",
                schema: "fortriskmanagement",
                table: "SavedSearches");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<Guid>(
                name: "SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FloodEvents_SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                column: "SavedSearchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodEvents_SavedSearches_SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                column: "SavedSearchId",
                principalSchema: "fortriskmanagement",
                principalTable: "SavedSearches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodEvents_SavedSearches_SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloodEvents_SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "SavedSearchId",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSearches_FloodEventId",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                column: "FloodEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodEvents_Reference",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                column: "Reference",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedSearches_FloodEvents_FloodEventId",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                column: "FloodEventId",
                principalSchema: "fortriskmanagement",
                principalTable: "FloodEvents",
                principalColumn: "Id");
        }
    }
}
