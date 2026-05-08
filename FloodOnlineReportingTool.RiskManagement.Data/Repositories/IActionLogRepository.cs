
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct);
}
