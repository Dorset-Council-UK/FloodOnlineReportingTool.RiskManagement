using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
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

    public async Task<SavedSearch?> GetSavedSearchByReference(string reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        reference = reference.ToUpperInvariant();
        return await context.SavedSearches.AsNoTracking()
                .Where(s => s.Reference == reference)
                .FirstOrDefaultAsync(ct);
    }

    public async Task<SavedSearch> GetOrCreateSavedSearch(Guid? id, string? reference, SavedSearch searchCriteria, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        reference = reference?.ToUpperInvariant();
        SavedSearch? existingSearch;

        // Test the ID based fields first
        if (id.HasValue && id.Value != Guid.Empty)
        {
            existingSearch = await context.SavedSearches.AsNoTracking()
                .Where(s => s.Id == id.Value)
                .FirstOrDefaultAsync(ct);
            if (existingSearch != null) return existingSearch;
        }
        if (string.IsNullOrEmpty(reference) == false)
        {
            existingSearch = await context.SavedSearches.AsNoTracking()
                .Where(s => s.Reference == reference)
                .FirstOrDefaultAsync(ct);
            if (existingSearch != null) return existingSearch;
        }
        if (string.IsNullOrEmpty(searchCriteria.SearchTitle) == false)
        {
            existingSearch = await context.SavedSearches.AsNoTracking()
                .Where(s => s.SearchTitle == searchCriteria.SearchTitle)
                .FirstOrDefaultAsync(ct);
            if (existingSearch != null) return existingSearch;
        }

        // Try to find existing search with matching criteria (exclude Id and SearchTitle)
        var primaryCauseIds = searchCriteria.FloodPrimaryCauses.OrderBy(x => x).ToList();
        var primaryCauseCount = primaryCauseIds.Count;

        // Database filtering: Simple fields + count + geometry (PostGIS)
        var candidates = await context.SavedSearches
            .AsNoTracking()
            .Where(s =>
                s.ResultsFrom == searchCriteria.ResultsFrom &&
                s.ResultsTo == searchCriteria.ResultsTo &&
                s.Postcode == searchCriteria.Postcode &&
                s.ReporterString == searchCriteria.ReporterString &&
                s.FloodPrimaryCauses.Count == primaryCauseCount &&
                ((s.Geom == null && searchCriteria.Geom == null) ||
                 (s.Geom != null && searchCriteria.Geom != null && s.Geom.EqualsTopologically(searchCriteria.Geom))))
            .ToListAsync(ct);

        // In-memory: Only check collection equality (can't translate SequenceEqual)
        existingSearch = candidates.FirstOrDefault(s =>
            s.FloodPrimaryCauses.OrderBy(x => x).SequenceEqual(primaryCauseIds));

        if (existingSearch != null) { return existingSearch; }

        // Create new saved search
        if (!string.IsNullOrEmpty(reference)) { searchCriteria.Reference = reference; }
        context.SavedSearches.Add(searchCriteria);
        await context.SaveChangesAsync(ct);

        return searchCriteria;
    }
}
