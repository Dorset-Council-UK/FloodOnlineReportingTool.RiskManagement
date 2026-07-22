using FloodOnlineReportingTool.Database.Models.Actions;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IEventLogRepository
{
    /// <summary>
    /// Create an event log for a saved search.
    /// </summary>
    Task<Result<EventLog>> CreateEventLog(Guid savedSearchId, ActionEventTypes eventLogType, CancellationToken ct);

    /// <summary>
    /// Create an event log for a flood event.
    /// </summary>
    Task<Result<EventLog>> CreateEventLogForFloodEvent(Guid floodEventId, ActionEventTypes eventLogType, CancellationToken cancellationToken);
}
