using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IEventLogRepository
{
    Task<CreateOrUpdateResult<EventLog>> CreateEventLog(Guid savedSearchId, ActionEventTypes eventLogType, CancellationToken ct);
}
