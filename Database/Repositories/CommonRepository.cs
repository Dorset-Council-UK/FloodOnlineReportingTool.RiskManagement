using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FloodOnlineReportingTool.Database.Repositories;

public class CommonRepository(IDbContextFactory<RiskManagementDbContext> contextFactory, IMemoryCache memoryCache) : ICommonRepository
{
    private const string FloodProblemsCacheKey = "FloodProblems";
    private const string DownstreamApiLayersCacheKey = "DownstreamApiLayers";
    private const string ReportStatusesCacheKey = "ReportStatuses";
    private const string FloodStatusesCacheKey = "FloodStatuses";
    private const string InvestigationStatusesCacheKey = "InvestigationStatuses";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct)
    {
        var cacheKey = $"{FloodProblemsCacheKey}:{category}";
        if (memoryCache.TryGetValue(cacheKey, out IList<FloodProblem>? cached)
            && cached is not null
            && cached.Count > 0)
        {
            return cached;
        }

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodProblems = await context.FloodProblems
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct);

        if (floodProblems.Count > 0)
        {
            memoryCache.Set(cacheKey, floodProblems, CacheDuration);
        }
        return floodProblems;
    }

    public async Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct)
    {
        var cacheKey = $"{DownstreamApiLayersCacheKey}:{apiSourceRecord}";
        if (memoryCache.TryGetValue(cacheKey, out DownstreamApiLayer? cached) && cached is not null)
        {
            return cached;
        }

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var layer = await context.DownstreamApiLayers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == apiSourceRecord, ct);

        if (layer is not null)
        {
            memoryCache.Set(cacheKey, layer, CacheDuration);
        }
        return layer;
    }

    public async Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(ReportStatusesCacheKey, out IList<ReportStatus>? cached)
            && cached is not null
            && cached.Count > 0)
        {
            return cached;
        }

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var statuses = await context.ReportStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);

        if (statuses.Count > 0)
        {
            memoryCache.Set(ReportStatusesCacheKey, statuses, CacheDuration);
        }
        return statuses;
    }

    public async Task<IList<FloodStatus>> GetFloodStatuses(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(FloodStatusesCacheKey, out IList<FloodStatus>? cached)
            && cached is not null
            && cached.Count > 0)
        {
            return cached;
        }

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var statuses = await context.FloodStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);

        if (statuses.Count > 0)
        {
            memoryCache.Set(FloodStatusesCacheKey, statuses, CacheDuration);
        }
        return statuses;
    }

    public async Task<IList<InvestigationStatus>> GetInvestigationStatuses(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(InvestigationStatusesCacheKey, out IList<InvestigationStatus>? cached)
            && cached is not null
            && cached.Count > 0)
        {
            return cached;
        }

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var statuses = await context.InvestigationStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);

        if (statuses.Count > 0)
        {
            memoryCache.Set(InvestigationStatusesCacheKey, statuses, CacheDuration);
        }
        return statuses;
    }
}
