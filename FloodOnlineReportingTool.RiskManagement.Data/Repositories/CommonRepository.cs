using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class CommonRepository(IDbContextFactory<RiskManagementDbContext> contextFactory) : ICommonRepository
{

    public async Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodProblems
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.DownstreamApiLayers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == apiSourceRecord, ct);
    }
}
