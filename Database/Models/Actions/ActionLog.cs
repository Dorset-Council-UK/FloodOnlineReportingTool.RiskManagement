using FloodOnlineReportingTool.Database.Models.Lookups;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.Database.Models.Actions;

/// <summary>
/// Logs actions from incoming and outgoing messages. This is used to track the history of actions taken on a report and to provide an audit trail for the report.
/// Geometry is included to allow for filtering actions by a users user range.
/// </summary>
public record ActionLog
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Taken from incoming messages
    public required string ReportReference { get; init; }
    public DateTimeOffset LoggedAt { get; init; } = DateTimeOffset.UtcNow;
    public MultiPolygon? PolygonGeom { get; init; }
    public Point? PointGeom { get; init; }

    // App defined properties
    public DateTimeOffset? ActionedAt { get; set; }
    public string? ActionedBy { get; set; }
    public string? Note { get; init; } // This will be used to describe unusual resolutions such as closing without completion by an admin. 

    public required Guid ActionId { get; init; }
    public required ActionDefinition Action { get; init; } 
}
