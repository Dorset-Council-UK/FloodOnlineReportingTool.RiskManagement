
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IActionLogRepository
{
    Task<CreateOrUpdateResult<ActionLog>> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct);

    Task<int> GetActionCountForUser(CancellationToken ct);
    Task<int> GetPropertyCountForUser(CancellationToken ct);

    Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct);

    Task<GetResult<ActionLog>> GetActionLogsByID(Guid actionID, CancellationToken ct);
}
