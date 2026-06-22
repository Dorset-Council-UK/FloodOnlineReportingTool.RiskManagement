using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IActionLogRepository
{
    Task<CreateOrUpdateResult<ActionLog>> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct);

    Task<int> GetActionCountForUser(CancellationToken ct);
    Task<int> GetPropertyCountForUser(CancellationToken ct);

    Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct);

    Task<PagedResult<ActionLog>> GetPagedActionLogsForUser(int pageNumber = 1, int pageSize = 10, string? sortColumn = null, bool ascending = true, CancellationToken ct = default);

    Task<GetResult<ActionLog>> GetActionLogById(Guid actionId, CancellationToken ct);

    Task<Guid?> GetAcknowledgeActionByReport(Guid Id, CancellationToken ct);

    Task<bool> AcknowledgeReport(Guid actionId, string currentUserDisplayName, CancellationToken ct);
}
