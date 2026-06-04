using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActionDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Lk_ActionDefinitions",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_ActionDefinitions", x => x.Id);
                },
                comment: "All possible actions and their user friendly descriptions");

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_ActionDefinitions",
                columns: new[] { "Id", "ActionType", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("076a4d83-4572-48b2-ae67-185c0877693f"), 1, "Request to archive source report", "TriggerFloodSourceArchive" },
                    { new Guid("123caad9-c1b9-4149-8f9e-3ef33dcd037b"), 1, "Request to delete flood event", "TriggerEventDelete" },
                    { new Guid("1a572d25-4dcb-464e-b4d4-b93a74d618aa"), 1, "Request to archive flood report", "TriggerFloodReportArchive" },
                    { new Guid("24b82488-37ed-4c36-8aae-82479611c3b5"), 2, "A new comment has been logged", "CommentNew" },
                    { new Guid("3f9f56fe-0ca5-4851-90ec-8f7a3082c95c"), 1, "Request to restore archived flood report", "TriggerFloodReportRestore" },
                    { new Guid("7afdfe7c-04a0-43d5-bf9b-abe53e1f2b1c"), 1, "Request to delete flood report", "TriggerFloodReportDelete" },
                    { new Guid("8067409e-7883-4fa9-a7cd-2ccd82ef4074"), 1, "Request to redact source report", "TriggerFloodSourceRedaction" },
                    { new Guid("a1b2c3d4-5e6f-4789-8abc-1234567890ab"), 2, "A request for a reporter to provide more information has been logged", "FurtherDetailRequested" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"), 0, "A flood report has been assigned to you", "ReportAssigned" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"), 0, "A flood event has been assigned to you", "EventAssigned" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567803"), 0, "Possible duplicate source report logged", "SourceDuplicateReview" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567804"), 0, "New source report logged", "SourceNew" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567805"), 0, "New flood report logged", "ReportNew" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567806"), 0, "A new flood event has been logged", "EventNew" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567807"), 0, "Update to a flood report logged", "ReportUpdate" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567808"), 0, "An update to a flood event has been logged", "EventUpdate" },
                    { new Guid("c2d3c27a-a0f6-46f0-b036-9a68b22e55f2"), 1, "Request to delete source report", "TriggerFloodSourceDelete" },
                    { new Guid("ccdcd417-1283-4545-988f-0fc7584bbda0"), 1, "Request to redact flood report", "TriggerFloodReportRedaction" },
                    { new Guid("d3510bd2-5141-4bcc-8f00-847cf8f9ef19"), 1, "Request to restore archived flood event", "TriggerEventRestore" },
                    { new Guid("d4e5f6a7-8192-4abc-bdef-4567890123de"), 0, "Work on a new section 19 investigation into flood event has been logged", "TriggerSection19" },
                    { new Guid("df655f85-fef9-4388-ab22-f10fd491657d"), 2, "The requested additional information has been provided", "FurtherDetailProvided" },
                    { new Guid("e310c7f5-2c0e-447f-aa27-6cb4c6aea033"), 0, "Section 19 investigation into flood event has been completed and report is available", "TriggerSection19Complete" },
                    { new Guid("e5f6a7b8-92a3-4bcd-cdef-5678901234ef"), 1, "Request to archive flood event", "TriggerEventArchive" },
                    { new Guid("f6a7b8c9-a3b4-4cde-def0-6789012345f0"), 2, "A reply to a comment has been logged", "CommentReply" },
                    { new Guid("febe4f8e-ca64-4e83-aabb-99bb15ae25ab"), 1, "Request to restore archived source report", "TriggerFloodSourceRestore" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                column: "ActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLogs_Lk_ActionDefinitions_ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                column: "ActionId",
                principalSchema: "fortriskmanagement",
                principalTable: "Lk_ActionDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionLogs_Lk_ActionDefinitions_ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.DropTable(
                name: "Lk_ActionDefinitions",
                schema: "fortriskmanagement");

            migrationBuilder.DropIndex(
                name: "IX_ActionLogs_ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "ActionId",
                schema: "fortriskmanagement",
                table: "ActionLogs");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "fortriskmanagement",
                table: "ActionLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
