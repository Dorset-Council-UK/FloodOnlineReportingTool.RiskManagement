using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.FloodEvents;

namespace FloodOnlineReportingTool.Database.Models.Actions;
public record EventLog
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Linked flood event
    public required Guid FloodEventId { get; init; }
    public FloodEvent FloodEvent { get; init; } = null!;

    public DateTimeOffset LoggedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ActionedAt { get; set; }
    public Guid? ActionedRequestByUserId { get; set; }

    public ActionEventTypes EventAction { get; init; }
}
