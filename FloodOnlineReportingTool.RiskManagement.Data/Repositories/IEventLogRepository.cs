using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

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
