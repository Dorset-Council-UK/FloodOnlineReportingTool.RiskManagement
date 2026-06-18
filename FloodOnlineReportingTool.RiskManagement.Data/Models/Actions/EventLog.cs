using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
public record EventLog
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Linked flood event
    public required Guid EventId { get; init; }
    public FloodEvent FloodEvent { get; init; } = null!;

    public DateTimeOffset LoggedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ActionedAt { get; set; }
    public Guid? ActionedRequestByUserId { get; set; }

    public ActionEventTypes EventAction { get; init; }
}
