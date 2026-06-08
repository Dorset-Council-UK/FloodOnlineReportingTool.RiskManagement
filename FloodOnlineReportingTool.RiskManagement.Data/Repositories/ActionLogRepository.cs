using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class ActionLogRepository(IDbContextFactory<RiskManagementDbContext> contextFactory) : IActionLogRepository
{
    public async Task<Result<ActionLog>> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Look up the 'ReportNew' action definition seeded into the database
        var actionDefinition = await context.ActionDefinitions
            .FirstOrDefaultAsync(a => a.Name == "ReportNew", ct);

        if (actionDefinition == null)
        {
            return Result<ActionLog>.Failure(["Action definition 'ReportNew' not found"]);
        }

        var newAction = new ActionLog
        {
            Id = Guid.NewGuid(),
            ReportId = incomingAction.Id,
            ReportReference = incomingAction.Reference,
            CreatedAt = incomingAction.CreatedUtc,
            PointGeom = new Point(incomingAction.Easting, incomingAction.Northing),
            LoggedAt = DateTimeOffset.UtcNow,
            ActionId = actionDefinition.Id,
            Action = actionDefinition,
        };

        context.ActionLogs.Add(newAction);
        await context.SaveChangesAsync(ct);

        return Result<ActionLog>.Success(newAction);
    }

    // TODO: Add filter by user functions. This should include filtering by:
    // - Organisation of the user
    // - User range assigned to the user
    // - The users permission level (e.g. only show actions that the user has permissions to fulfil)

    public async Task<int> GetActionCountForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter

        return await context.ActionLogs.CountAsync(ct);
    }

    public async Task<int> GetPropertyCountForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        return await context.ActionLogs
            .Where(a => a.Action.Name == "ReportNew")
            .Where(a => a.LoggedAt >= thirtyDaysAgo && a.LoggedAt <= DateTimeOffset.UtcNow)
            .CountAsync(ct);

    }

    public async Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter

        var actionLogs = await context.ActionLogs
            .Include(al => al.Action)
            .OrderByDescending(a => a.Action.Name)
            .ToListAsync(ct);
        return actionLogs;
    }

    public async Task<PagedResult<ActionLog>> GetPagedActionLogsForUser(int pageNumber = 1, int pageSize = 10, string? sortColumn = null, bool ascending = true, CancellationToken ct = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO: Insert user filter

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Apply sort based on column name, defaulting to LoggedAt descending
        IQueryable<ActionLog> query = context.ActionLogs.Include(al => al.Action).AsNoTracking();

        query = (sortColumn, ascending) switch
        {
            ("Action", true)                    => query.OrderBy(a => a.Action.Name),
            ("Action", false)                   => query.OrderByDescending(a => a.Action.Name),
            ("Flood Report Reference", true)    => query.OrderBy(a => a.ReportReference),
            ("Flood Report Reference", false)   => query.OrderByDescending(a => a.ReportReference),
            ("Created On", true)                => query.OrderBy(a => a.CreatedAt),
            ("Created On", false)               => query.OrderByDescending(a => a.CreatedAt),
            ("Logged On", true)                 => query.OrderBy(a => a.LoggedAt),
            ("Logged On", false)                => query.OrderByDescending(a => a.LoggedAt),
            ("Notes", true)                     => query.OrderBy(a => a.Note),
            ("Notes", false)                    => query.OrderByDescending(a => a.Note),
            _                                   => query.OrderByDescending(a => a.LoggedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var skip = (pageNumber - 1) * pageSize;

        var results = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var totalPages = (totalCount + pageSize - 1) / pageSize;

        return new PagedResult<ActionLog>(results, totalCount, pageSize, pageNumber, totalPages);
    }

    public async Task<GetResult<ActionLog>> GetActionLogById(Guid actionId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        // TODO insert user filter
        var actionLog = await context.ActionLogs
            .Include(al => al.Action)
            .Where(a => a.Id == actionId)
            .FirstOrDefaultAsync(ct);

        return actionLog is not null ? GetResult<ActionLog>.Success(actionLog) : GetResult<ActionLog>.Failure(new[] { "Action log not found" });
    }
}
