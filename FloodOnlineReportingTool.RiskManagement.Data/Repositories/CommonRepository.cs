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
    public async Task<SavedSearch> GetOrCreateSavedSearch(SavedSearch searchCriteria, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        
        // Try to find existing search with matching criteria (exclude Id and SearchTitle)
        var existingSearch = await context.SavedSearches
            .AsNoTracking()
            .Where(s => 
                s.ResultsFrom == searchCriteria.ResultsFrom &&
                s.ResultsTo == searchCriteria.ResultsTo &&
                s.Postcode == searchCriteria.Postcode &&
                s.ReporterString == searchCriteria.ReporterString &&
                s.ShowResultsAs == searchCriteria.ShowResultsAs)
            .FirstOrDefaultAsync(ct);

        if (existingSearch != null)
        {
            // Additional in-memory checks for collections and geometry
            bool causesMatch = existingSearch.FloodPrimaryCauses
                .OrderBy(x => x)
                .SequenceEqual(searchCriteria.FloodPrimaryCauses.OrderBy(x => x));

            bool geomMatches = (existingSearch.Geom == null && searchCriteria.Geom == null) ||
                (existingSearch.Geom?.EqualsExact(searchCriteria.Geom) == true);

            if (causesMatch && geomMatches)
            {
                return existingSearch;
            }
        }

        // Create new saved search
        context.SavedSearches.Add(searchCriteria);
        await context.SaveChangesAsync(ct);

        return searchCriteria;
    }
}
