using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
}
