using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class CommonRepository(
    IDbContextFactory<RiskManagementDbContext> contextFactory
    ) : ICommonRepository
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

    public async Task<GetResult<SearchRedirect>> FindLocalByReference(string reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Run all local DB checks concurrently.
        var savedSearchTask = await context.SavedSearches
            .AsNoTracking()
            .Where(s => s.Reference == reference)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);
        var floodEventTask = await context.FloodEvents
            .AsNoTracking()
            .Where(f => f.Reference == reference)
            .Select(f => f.Id)
            .FirstOrDefaultAsync(ct);

        if (savedSearchTask != Guid.Empty)
            return GetResult<SearchRedirect>.Success(new SearchRedirect
            {
                Id = savedSearchTask,
                Reference = reference,
                RedirectType = RecordTypes.Search
            });

        if (floodEventTask != Guid.Empty)
            return GetResult<SearchRedirect>.Success(new SearchRedirect
            {
                Id = floodEventTask,
                Reference = reference,
                RedirectType = RecordTypes.Event
            });

        return GetResult<SearchRedirect>.Failure([$"No local record found for reference '{reference}'."]);
    }

    public async Task<SavedSearch?> GetSavedSearch(string reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.SavedSearches
            .AsNoTracking()
            .Where(s => s.Reference == reference)
            .Include(s => s.FloodEvent)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SavedSearch?> GetSavedSearch(Guid savedSearchId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.SavedSearches
            .AsNoTracking()
            .Where(s => s.Id == savedSearchId)
            .Include(s => s.FloodEvent)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SavedSearch> GetOrCreateSavedSearch(Guid? id, string? reference, SavedSearch searchCriteria, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
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
        var primaryCauseIds = searchCriteria.FloodSources.OrderBy(x => x).ToList();
        var primaryCauseCount = primaryCauseIds.Count;

        // Database filtering: Simple fields + count + geometry (PostGIS)
        var candidates = await context.SavedSearches
            .AsNoTracking()
            .Where(s =>
                s.ResultsFrom == searchCriteria.ResultsFrom &&
                s.ResultsTo == searchCriteria.ResultsTo &&
                s.Postcode == searchCriteria.Postcode &&
                s.ReporterString == searchCriteria.ReporterString &&
                s.FloodSources.Count == primaryCauseCount &&
                ((s.Geom == null && searchCriteria.Geom == null) ||
                 (s.Geom != null && searchCriteria.Geom != null && s.Geom.EqualsTopologically(searchCriteria.Geom))))
            .ToListAsync(ct);

        // In-memory: Only check collection equality (can't translate SequenceEqual)
        existingSearch = candidates.FirstOrDefault(s =>
            s.FloodSources.OrderBy(x => x).SequenceEqual(primaryCauseIds));

        if (existingSearch != null)
        {
            return existingSearch;
        }

        // Create new saved search
        context.SavedSearches.Add(searchCriteria);
        await context.SaveChangesAsync(ct);

        return searchCriteria;
    }

    public async Task<SavedSearch> UpdateSavedSearch(string reference, SavedSearch searchCriteria, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var existingSearch = await context.SavedSearches.AsNoTracking()
            .Where(s => s.Reference == reference)
            .FirstOrDefaultAsync(ct);

        if (existingSearch == null)
        {
            return searchCriteria;
        }

        var updatedSearch = existingSearch with
        {
            Id = existingSearch.Id, // Ensure ID remains unchanged
            Reference = existingSearch.Reference, // Ensure Reference remains unchanged
            FloodEventId = existingSearch.FloodEventId, // Ensure FloodEventId remains unchanged
            SearchTitle = searchCriteria.SearchTitle,
            ResultsFrom = searchCriteria.ResultsFrom,
            ResultsTo = searchCriteria.ResultsTo,
            FloodSources = searchCriteria.FloodSources,
            UPRN = searchCriteria.UPRN,
            USRN = searchCriteria.USRN,
            Postcode = searchCriteria.Postcode,
            ReporterString = searchCriteria.ReporterString,
            Geom = searchCriteria.Geom,
            PagingSize = searchCriteria.PagingSize,
            SortBy = searchCriteria.SortBy,
            SortOrder = searchCriteria.SortOrder,
            LimitByOrganisation = searchCriteria.LimitByOrganisation,
            LimitByStatus = searchCriteria.LimitByStatus,
            ShowResultsAs = searchCriteria.ShowResultsAs
        };

        // TODO - this works but can result in duplicate searches. We need to handle this. 

        context.SavedSearches.Update(updatedSearch);
        await context.SaveChangesAsync(ct);

        return updatedSearch;
    }

    public async Task<bool> SetSavedSearchTitle(Guid id, string Title, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var existingSearch = await context.SavedSearches
                .Where(s => s.Id ==id)
                .FirstOrDefaultAsync(ct);
        if (existingSearch == null)
        {
            return false;
        }
        existingSearch.SearchTitle = Title;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ReportStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);
    }

    public async Task<IList<FloodStatus>> GetFloodStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);
    }

    public async Task<IList<InvestigationStatus>> GetInvestigationStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.InvestigationStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);
    }
}
