using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FloodOnlineReportingTool.RiskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fortriskmanagement");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "FloodEvents",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MitigationTaken = table.Column<int>(type: "integer", nullable: false),
                    RecordStatus = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodStatus = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestigationStatus = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimatedVulnerable = table.Column<int>(type: "integer", nullable: true),
                    EstimatedUninhabitable = table.Column<int>(type: "integer", nullable: true),
                    EstimatedNoElectric = table.Column<int>(type: "integer", nullable: true),
                    EstimatedNoGas = table.Column<int>(type: "integer", nullable: true),
                    EstimatedNoFoul = table.Column<int>(type: "integer", nullable: true),
                    EstimatedNoWaterSupply = table.Column<int>(type: "integer", nullable: true),
                    EstimatedNoPhone = table.Column<int>(type: "integer", nullable: true),
                    EstimatedDisruptedUtilities = table.Column<int>(type: "integer", nullable: true),
                    EstimatedResidentialAffected = table.Column<int>(type: "integer", nullable: true),
                    EstimatedResidentialFlooded = table.Column<int>(type: "integer", nullable: true),
                    EstimatedResidentialOther = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCommercialAffected = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCommercialFlooded = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCommercialOther = table.Column<int>(type: "integer", nullable: true),
                    EstimatedTotalExternalOnly = table.Column<int>(type: "integer", nullable: true),
                    EstimatedTotalInternallyFlooded = table.Column<int>(type: "integer", nullable: true),
                    StatusNote = table.Column<string>(type: "text", nullable: false),
                    FloodExtentConfidence = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "Lk_CommentTypes",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_CommentTypes", x => x.Id);
                },
                comment: "All possible comment types and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "Lk_EventStatuses",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_EventStatuses", x => x.Id);
                },
                comment: "All possible event statuses and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "Lk_FloodProblems",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    TypeDescription = table.Column<string>(type: "text", nullable: true),
                    OptionOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_FloodProblems", x => x.Id);
                },
                comment: "All possible flood problems and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "Lk_FloodStatuses",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_FloodStatuses", x => x.Id);
                },
                comment: "All possible flood statuses and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "Lk_InvestigationStatuses",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_InvestigationStatuses", x => x.Id);
                },
                comment: "All possible investigation statuses and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "Lk_ReportStatuses",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lk_ReportStatuses", x => x.Id);
                },
                comment: "All possible report statuses and their user friendly descriptions");

            migrationBuilder.CreateTable(
                name: "OutboxState",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "FloodEventComments",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentType = table.Column<int>(type: "integer", nullable: false),
                    CommentText = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    RepliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RepliedBy = table.Column<string>(type: "text", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadBy = table.Column<string>(type: "text", nullable: false),
                    FloodEventId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEventComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodEventComments_FloodEvents_FloodEventId",
                        column: x => x.FloodEventId,
                        principalSchema: "fortriskmanagement",
                        principalTable: "FloodEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedSearches",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchTitle = table.Column<string>(type: "text", nullable: false),
                    ResultsFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultsTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FloodPrimaryCauses = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    FloodSecondaryCauses = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    UPRN = table.Column<long>(type: "bigint", nullable: true),
                    USRN = table.Column<long>(type: "bigint", nullable: true),
                    Postcode = table.Column<string>(type: "text", nullable: false),
                    ReporterString = table.Column<string>(type: "text", nullable: false),
                    Geom = table.Column<MultiPolygon>(type: "geometry", nullable: true),
                    SearchRadius = table.Column<double>(type: "double precision", nullable: true),
                    PagingSize = table.Column<int>(type: "integer", nullable: true),
                    SortBy = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    LimitByOrganisation = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    LimitByStatus = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    ShowResultsAs = table.Column<int>(type: "integer", nullable: false),
                    FloodEventId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedSearches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedSearches_FloodEvents_FloodEventId",
                        column: x => x.FloodEventId,
                        principalSchema: "fortriskmanagement",
                        principalTable: "FloodEvents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "fortriskmanagement",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalSchema: "fortriskmanagement",
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalSchema: "fortriskmanagement",
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_CommentTypes",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { 0, "A comment made by a member of the public", 1, "Public Comment" },
                    { 1, "A comment made by a logged in user", 2, "Logged in User Comment" },
                    { 2, "A comment describing problems caused by the flood", 3, "Problem Comment" },
                    { 3, "A comment about the impact of the flood event", 4, "Impact Comment" },
                    { 4, "A comment providing a description of the flood event", 5, "Description Comment" },
                    { 5, "A comment about the history of the flood event", 6, "History Comment" },
                    { 6, "A comment about the cause of the flood event", 7, "Cause Comment" },
                    { 7, "Details about the source of the flood event", 8, "Source Details" },
                    { 8, "A comment about assistance provided during the flood event", 9, "Assistance Comment" },
                    { 9, "A comment made by an administrator", 10, "Admin Comment" }
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_EventStatuses",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { new Guid("018feb46-3d00-72a8-a3e1-056e99014150"), "A forecast of/imminent risk of buildings/land being flooded", 1, "Prepare Phase" },
                    { new Guid("018feb47-2760-7161-af3d-6c1036e802ed"), "Buildings/land are currently flooded", 2, "Response Phase" },
                    { new Guid("018feb48-11c0-7fd1-a032-e87cae11748c"), "A recent flood event where buildings/land are no longer flooded but remedial work to properties is on-going", 3, "Recovery Phase" },
                    { new Guid("018feb48-fc20-7d06-b7cd-b4440af84719"), "A past flood event where homes/businesses/land are no longer affected", 4, "Analyse Phase" }
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_FloodProblems",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[,]
                {
                    { new Guid("018fe08b-a800-7cb2-aa1b-f39aefc48152"), "Primary", 1, "Flooding caused by a river overflowing its banks", "River Flooding" },
                    { new Guid("018fe08c-9260-77ad-9e3a-418bb047ff5d"), "Primary", 2, "Flooding caused by a stream or watercourse overflowing its banks", "Stream or Watercourse Flooding" },
                    { new Guid("018fe08d-7cc0-70ce-89e3-78360019c9b7"), "Primary", 3, "Flooding caused by a lake or reservoir overflowing its banks", "Lake or Reservoir Flooding" },
                    { new Guid("018fe08e-6720-70ea-9a72-bf9aa0de312b"), "Primary", 4, "Flooding caused by the sea, including coastal flooding and tidal surges", "The Sea" },
                    { new Guid("018fe08f-5180-744d-ab20-b58bf6d04fbb"), "Primary", 5, "Flooding caused by ditches and drainage channels overflowing or blocked", "Ditches and Drainage Channels" },
                    { new Guid("018fe090-3be0-7d28-b73d-ba671aa61208"), "Primary", 6, "Flooding caused by water rising up from the ground, such as from a high water table or underground springs", "Water Rising Out of the Ground" },
                    { new Guid("018fe091-2640-7a8c-ab1e-5c762268a1d7"), "Primary", 7, "Flooding caused by foul drainage or sewerage systems overflowing or blocked", "Foul Drainage/Sewerage" },
                    { new Guid("018fe092-10a0-7858-8987-1f84395524b7"), "Primary", 8, "Flooding caused by surface water drainage systems overflowing or blocked", "Surface Water Drainage" },
                    { new Guid("018fe092-fb00-7a34-a8c9-bdac73a1acaa"), "Primary", 9, "Flooding caused by blocked road drainage systems", "Blocked Road Drainage" },
                    { new Guid("018fe093-e560-74dc-ac18-f4949190dce3"), "Primary", 10, "Flooding caused by a bridge or culvert being blocked or overtopped", "Bridge or Culvert" },
                    { new Guid("018fe094-cfc0-7156-99f5-2fc20e9e19ea"), "Primary", 11, "Flooding caused by waves from passing vehicles, such as on a busy road or near a construction site", "Waves Caused by Vehicles" },
                    { new Guid("018fe095-ba20-7234-b73b-ff8e340dd9fc"), "Primary", 12, "Flooding caused by rainwater flowing over the ground, such as during heavy rainfall or thunderstorms", "Rainwater Flowing Over the Ground" },
                    { new Guid("018fe096-a480-70d8-91f4-03504bcf926c"), "Primary", 13, "The cause of flooding is not sure or not known", "Not Sure" },
                    { new Guid("018fe0c2-9680-78b5-a5fd-ca2bb2ddd0e3"), "Secondary", 20, "Flooding caused by runoff from a road", "Runoff from Road" },
                    { new Guid("018fe0c3-80e0-7a22-a8d4-36ad1e5dd626"), "Secondary", 21, "Flooding caused by runoff from a private road", "Runoff from Private Road" },
                    { new Guid("018fe0c4-6b40-7441-afbb-381e233f4906"), "Secondary", 22, "Flooding caused by runoff from a track or path", "Runoff from Track or Path" },
                    { new Guid("018fe0c5-55a0-7991-8ee8-1df41519d18e"), "Secondary", 23, "Flooding caused by runoff from agricultural land", "Runoff from Agricultural Land" },
                    { new Guid("018fe0c6-4000-7e95-84d4-1ad96cf4f598"), "Secondary", 24, "Flooding caused by runoff from another property", "Runoff from Other Property" },
                    { new Guid("018fe0c7-2a60-7983-b7c3-afa68072aa5f"), "Secondary", 25, "The cause of flooding is not sure or not known", "Not Sure" }
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_FloodStatuses",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { new Guid("018feb7d-2b80-7da6-bd00-dd2c83fa2a2e"), null, 1, "Flood Expected: No Flood" },
                    { new Guid("018feb7e-15e0-7a9d-ac21-bc1cc63e081c"), null, 2, "Flood Expected: Help Given" },
                    { new Guid("018feb7f-0040-7d3f-ae3c-796c50850cbe"), null, 3, "Properties Affected" },
                    { new Guid("018feb7f-eaa0-7507-af59-497aac53467d"), null, 4, "Properties Affected: Help Given" },
                    { new Guid("018feb80-d500-7aa7-b155-b6c3218ff2cc"), null, 5, "Buildings Flooded" },
                    { new Guid("018feb81-bf60-788d-86c9-ec5f1cc1871c"), null, 6, "Buildings Flooded: Help Given" },
                    { new Guid("018feb82-a9c0-7d33-bbf1-64422e309748"), null, 7, "No Flooding Occurred" }
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_InvestigationStatuses",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { new Guid("018febeb-0880-7532-b583-3d9502dffd7b"), "No Section 19 report", 1, "No Investigation" },
                    { new Guid("018febeb-f2e0-7d1d-89a3-b76b1fe98343"), "Section 19 report required", 2, "Investigation Required" },
                    { new Guid("018febec-dd40-7863-b45c-e7b9546f588c"), "Section 19 report in progress", 3, "In Progress" },
                    { new Guid("018febed-c7a0-7994-8c44-4623142fdfb1"), "Included in Section 19 report", 4, "Report Available" }
                });

            migrationBuilder.InsertData(
                schema: "fortriskmanagement",
                table: "Lk_ReportStatuses",
                columns: new[] { "Id", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { new Guid("018feb0f-4e80-767d-8262-36a1217ae690"), "The record is marked for deletion and will be removed from the system in 48 hours", 99, "Marked for Deletion" },
                    { new Guid("018feb10-38e0-7f30-a546-37ce71f243ae"), "This is a new record that has not been viewed yet", 1, "New" },
                    { new Guid("018feb11-2340-7449-9a20-83e2043a6817"), "This record has been viewed but no action has been taken yet", 2, "Viewed" },
                    { new Guid("018feb12-0da0-749b-a59a-cb3ed128d982"), "Action is needed on this record and needs to be reviewed", 3, "Action needed" },
                    { new Guid("018feb12-f800-75e0-ab95-e780864249c8"), "Action has been completed on this record", 4, "Action completed" },
                    { new Guid("018feb13-e260-7c11-9106-c179ba7c8ce4"), "This record has an error and needs to be reviewed", 5, "Error" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodEventComments_FloodEventId",
                schema: "fortriskmanagement",
                table: "FloodEventComments",
                column: "FloodEventId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "fortriskmanagement",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                schema: "fortriskmanagement",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                schema: "fortriskmanagement",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "fortriskmanagement",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                schema: "fortriskmanagement",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                schema: "fortriskmanagement",
                table: "OutboxState",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSearches_FloodEventId",
                schema: "fortriskmanagement",
                table: "SavedSearches",
                column: "FloodEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloodEventComments",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_CommentTypes",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_EventStatuses",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_FloodProblems",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_FloodStatuses",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_InvestigationStatuses",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "Lk_ReportStatuses",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "SavedSearches",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "fortriskmanagement");

            migrationBuilder.DropTable(
                name: "FloodEvents",
                schema: "fortriskmanagement");
        }
    }
}
