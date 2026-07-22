using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.Database.Models.Actions;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IActionLogRepository
{
    /// <summary>
    /// Creates an action log from the FloodReportCreated message contract.
    /// </summary>
    /// <returns>A result pattern with the created action log, or a list of errors.</returns>
    Task<Result<ActionLog>> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct);

    Task<int> GetActionCountForUser(CancellationToken ct);
    Task<int> GetPropertyCountForUser(CancellationToken ct);

    Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct);

    Task<PagedResult<ActionLog>> GetPagedActionLogsForUser(int pageNumber = 1, int pageSize = 10, string? sortColumn = null, bool ascending = true, CancellationToken ct = default);

    Task<GetResult<ActionLog>> GetActionLogById(Guid actionId, CancellationToken ct);

    Task<Guid?> GetAcknowledgeActionByReport(string reportRef, CancellationToken ct);

    Task<bool> AcknowledgeReport(Guid actionId, string currentUserDisplayName, CancellationToken ct);
}
