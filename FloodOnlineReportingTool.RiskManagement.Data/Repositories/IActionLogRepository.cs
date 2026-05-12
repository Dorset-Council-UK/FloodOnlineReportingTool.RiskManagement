
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct);

    Task<int> GetActionCountForUser(CancellationToken ct);
    Task<int> GetPropertyCountForUser(CancellationToken ct);

    Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct);
}
