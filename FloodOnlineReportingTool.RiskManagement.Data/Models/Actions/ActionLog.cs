using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;

/// <summary>
/// Logs actions from incoming and outgoing messages. This is used to track the history of actions taken on a report and to provide an audit trail for the report.
/// Geometry is included to allow for filtering actions by a users user range.
/// </summary>
public record ActionLog
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Taken from incoming messages
    public required Guid ReportId { get; init; }
    public required string ReportReference { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public MultiPolygon? PolygonGeom { get; init; }
    public Point? PointGeom { get; init; }

    // App defined properties
    public DateTimeOffset LoggedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Action { get; init; } = string.Empty;
    public DateTimeOffset? ActionedAt { get; set; }
    public Guid? ActionedByUserId { get; set; }
    public string? Note { get; init; } // This will be used to describe unusual resolutions such as closing without completion by an admin. 
}
