using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class ActionLogRepository(IDbContextFactory<RiskManagementDbContext> contextFactory) : IActionLogRepository
{
    public async Task<ActionLog> CreateActionLog(FloodReportCreated incomingAction, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var newAction = new ActionLog
        {
            Id = Guid.NewGuid(),
            ReportId = incomingAction.Id,
            ReportReference = incomingAction.Reference,
            CreatedAt = incomingAction.CreatedUtc,
            PointGeom = new Point(incomingAction.Easting, incomingAction.Northing),
            LoggedAt = DateTimeOffset.UtcNow,
            Action = "New Report Logged",
        };

        context.ActionLogs.Add(newAction);
        await context.SaveChangesAsync(ct);

        return newAction;
    }

    // TODO: Add filter by user functions. This should include filtering by:
    // - Organisation of the user
    // - User range assigned to the user
    // - The users permission level (e.g. only show actions that the user has permissions to fulfil)

    public async Task<int> GetActionCountForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter

        return await context.ActionLogs.CountAsync();
    }

    public async Task<int> GetPropertyCountForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        return await context.ActionLogs
            .Where(a => a.Action == "New Report Logged")
            .Where(a => a.LoggedAt >= thirtyDaysAgo && a.LoggedAt <= DateTimeOffset.UtcNow)
            .CountAsync();

    }

    public async Task<IReadOnlyCollection<ActionLog>> GetActionLogsForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter

        var actionLogs = await context.ActionLogs
            .OrderByDescending(a => a.Action)
            .ToListAsync(ct);
        return actionLogs;
    }
}
