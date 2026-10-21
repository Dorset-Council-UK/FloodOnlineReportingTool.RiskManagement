using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaUrlReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SortBy",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "LimitByStatus",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "LimitByOrganisation",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "FloodSources",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]");

            migrationBuilder.AddColumn<string>(
                name: "MediaReference",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MediaReferenceExpiration",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MediaUploadLock",
                schema: "fortriskmanagement",
                table: "FloodEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaReference",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "MediaReferenceExpiration",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.DropColumn(
                name: "MediaUploadLock",
                schema: "fortriskmanagement",
                table: "FloodEvents");

            migrationBuilder.AlterColumn<string>(
                name: "SortBy",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "LimitByStatus",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0],
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid[]>(
                name: "LimitByOrganisation",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0],
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid[]>(
                name: "FloodSources",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0],
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldNullable: true);
        }
    }
}
