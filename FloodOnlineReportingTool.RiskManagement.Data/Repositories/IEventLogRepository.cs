using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IEventLogRepository
{
    Task<CreateOrUpdateResult<EventLog>> CreateEventLog(Guid FloodEventId, ActionEventTypes EventLogType, CancellationToken ct);
    Task<GetResult<EventLog>> GetEventLogById(Guid eventLogId, CancellationToken ct);
}
