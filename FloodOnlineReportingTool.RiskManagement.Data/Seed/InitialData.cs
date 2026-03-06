using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using RecordStatusIds = FloodOnlineReportingTool.Contracts.Shared.RecordStatusIds;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Data.Seed;

/// <summary>
///     <para>The initial data to seed the database with. This is applied in the migration.</para>
///     <para>"you should use explicit Guids when seeding with HasData, never generated ones" <see href="https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/august/data-points-deep-dive-into-ef-core-hasdata-seeding#what-about-private-setters">Deep Dive into EF Core HasData Seeding</see></para>
/// </summary>
internal static class InitialData
{
    internal static EventStatus[] EventStatusData()
    {
        return [
            new EventStatus(EventStatusIds.PreparePhase, "Prepare Phase", 1, "A forecast of/imminent risk of buildings/land being flooded"),
            new EventStatus(EventStatusIds.ResponsePhase, "Response Phase", 2, "Buildings/land are currently flooded"),
            new EventStatus(EventStatusIds.RecoveryPhase, "Recovery Phase", 3, "A recent flood event where buildings/land are no longer flooded but remedial work to properties is on-going"),
            new EventStatus(EventStatusIds.AnalysePhase, "Analyse Phase", 4, "A past flood event where homes/businesses/land are no longer affected")     
        ];
    }

    internal static FloodStatus[] FloodStatusData()
    {
        return [
            new FloodStatus(EventStatusIds.FloodExpectedNoFlood, "Flood Expected: No Flood", 1),
            new FloodStatus(EventStatusIds.FloodExpectedHelpGiven, "Flood Expected: Help Given", 2),
            new FloodStatus(EventStatusIds.PropertiesAffected, "Properties Affected", 3),
            new FloodStatus(EventStatusIds.PropertiesAffectedHelpGiven, "Properties Affected: Help Given", 4),
            new FloodStatus(EventStatusIds.BuildingsFlooded, "Buildings Flooded", 5),
            new FloodStatus(EventStatusIds.BuildingsFloodedHelpGiven, "Buildings Flooded: Help Given", 6),
            new FloodStatus(EventStatusIds.NoFloodingOccurred, "No Flooding Occurred", 7)
        ];
    }

    internal static ReportStatus[] ReportStatusData()
    {
        return [
            new ReportStatus(RecordStatusIds.MarkedForDeletion, "Marked for Deletion", 99, "The record is marked for deletion and will be removed from the system in 48 hours"),
            new ReportStatus(RecordStatusIds.New, "New", 1, "This is a new record that has not been viewed yet"),
            new ReportStatus(RecordStatusIds.Viewed, "Viewed", 2, "This record has been viewed but no action has been taken yet"),
            new ReportStatus(RecordStatusIds.ActionNeeded, "Action needed", 3, "Action is needed on this record and needs to be reviewed"),
            new ReportStatus(RecordStatusIds.ActionCompleted, "Action completed", 4, "Action has been completed on this record"),
            new ReportStatus(RecordStatusIds.Error, "Error", 5, "This record has an error and needs to be reviewed")
        ];
    }

    internal static InvestigationStatus[] InvestigationStatusData()
    {
        return [
            new InvestigationStatus(Section19StatusIds.NoSection19,"No Investigation", "No Section 19 report", 1),
            new InvestigationStatus(Section19StatusIds.Section19Required,"Investigation Required", "Section 19 report required", 2),
            new InvestigationStatus(Section19StatusIds.Section19InProgress,"In Progress", "Section 19 report in progress", 3),
            new InvestigationStatus(Section19StatusIds.Section19Included,"Report Available", "Included in Section 19 report", 4)
        ];
    }

    internal static FloodProblem[] FloodProblemData()
    {
        return [
            new FloodProblem(PrimaryCauseIds.River, "Primary", "River Flooding", "Flooding caused by a river overflowing its banks", 1),
            new FloodProblem(PrimaryCauseIds.StreamOrWatercourse, "Primary", "Stream or Watercourse Flooding", "Flooding caused by a stream or watercourse overflowing its banks", 2),
            new FloodProblem(PrimaryCauseIds.LakeOrReservoir, "Primary", "Lake or Reservoir Flooding", "Flooding caused by a lake or reservoir overflowing its banks", 3),
            new FloodProblem(PrimaryCauseIds.TheSea, "Primary", "The Sea", "Flooding caused by the sea, including coastal flooding and tidal surges", 4),
            new FloodProblem(PrimaryCauseIds.DitchesAndDrainageChannels, "Primary", "Ditches and Drainage Channels", "Flooding caused by ditches and drainage channels overflowing or blocked", 5),
            new FloodProblem(PrimaryCauseIds.WaterRisingOutOfTheGround, "Primary", "Water Rising Out of the Ground", "Flooding caused by water rising up from the ground, such as from a high water table or underground springs", 6),
            new FloodProblem(PrimaryCauseIds.FoulDrainageSewerage, "Primary", "Foul Drainage/Sewerage", "Flooding caused by foul drainage or sewerage systems overflowing or blocked", 7),
            new FloodProblem(PrimaryCauseIds.SurfaceWaterDrainage, "Primary", "Surface Water Drainage", "Flooding caused by surface water drainage systems overflowing or blocked", 8),
            new FloodProblem(PrimaryCauseIds.BlockedRoadDrainage, "Primary", "Blocked Road Drainage", "Flooding caused by blocked road drainage systems", 9),
            new FloodProblem(PrimaryCauseIds.BridgeOrCulvert, "Primary", "Bridge or Culvert", "Flooding caused by a bridge or culvert being blocked or overtopped", 10),
            new FloodProblem(PrimaryCauseIds.WavesCausedByVehicles, "Primary", "Waves Caused by Vehicles", "Flooding caused by waves from passing vehicles, such as on a busy road or near a construction site", 11),
            new FloodProblem(PrimaryCauseIds.RainwaterFlowingOverTheGround, "Primary", "Rainwater Flowing Over the Ground", "Flooding caused by rainwater flowing over the ground, such as during heavy rainfall or thunderstorms", 12),
            new FloodProblem(PrimaryCauseIds.NotSure, "Primary", "Not Sure", "The cause of flooding is not sure or not known", 13),

            // Secondary causes of flooding (only applicable if the primary cause is 'Runoff')
            new FloodProblem(SecondaryCauseIds.RunoffFromRoad, "Secondary", "Runoff from Road", "Flooding caused by runoff from a road", 20),
            new FloodProblem(SecondaryCauseIds.RunoffFromPrivateRoad, "Secondary", "Runoff from Private Road", "Flooding caused by runoff from a private road", 21),
            new FloodProblem(SecondaryCauseIds.RunoffFromTrackOrPath, "Secondary", "Runoff from Track or Path", "Flooding caused by runoff from a track or path", 22),
            new FloodProblem(SecondaryCauseIds.RunoffFromAgriculturalLand, "Secondary", "Runoff from Agricultural Land", "Flooding caused by runoff from agricultural land", 23),
            new FloodProblem(SecondaryCauseIds.RunoffFromOtherProperty, "Secondary", "Runoff from Other Property", "Flooding caused by runoff from another property", 24),
            new FloodProblem(SecondaryCauseIds.NotSure, "Secondary", "Not Sure", "The cause of flooding is not sure or not known", 25)
        ];
    }

    internal static Models.Lookups.CommentType[] CommentTypeData()
    {
        return [
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.PublicComment, "Public Comment", 1, "A comment made by a member of the public"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.UserComment, "Logged in User Comment", 2, "A comment made by a logged in user"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.ProblemComment, "Problem Comment", 3, "A comment describing problems caused by the flood"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.ImpactComment, "Impact Comment", 4, "A comment about the impact of the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.DescriptionComment, "Description Comment", 5, "A comment providing a description of the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.HistoryComment, "History Comment", 6, "A comment about the history of the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.CauseComment, "Cause Comment", 7, "A comment about the cause of the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.SourceDetails, "Source Details", 8, "Details about the source of the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.AssistanceComment, "Assistance Comment", 9, "A comment about assistance provided during the flood event"),
            new Models.Lookups.CommentType((int)Contracts.Shared.CommentType.AdminComment, "Admin Comment", 10, "A comment made by an administrator")
        ];
    }

    internal static DownstreamApiLayer[] DownstreamApiLayerData()
    {
        return [
            new DownstreamApiLayer("parish", "GeoServer", "/geoserver/ows", "", "name", RequestMethods.WFS, RequestVersions.V1_0_0, "name", SortOptions.Ascending),
            new DownstreamApiLayer("ward", "GeoServer", "/geoserver/ows", "", "name", RequestMethods.WFS, RequestVersions.V1_0_0, "name", SortOptions.Ascending)
        ];
    }

}
