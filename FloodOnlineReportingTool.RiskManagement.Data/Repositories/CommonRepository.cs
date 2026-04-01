using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        return await context.SavedSearches.AsNoTracking()
                .Where(s => s.Reference == reference)
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

    public async Task<SavedSearch?> GetFloodEvent(Guid? SavedSearchId, string? reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        reference = reference?.ToUpperInvariant();
        SavedSearch? existingSearch;

        // Test the ID based fields first
        if (SavedSearchId.HasValue && SavedSearchId.Value != Guid.Empty)
        {
            existingSearch = await context.SavedSearches.AsNoTracking()
                .Where(s => s.Id == SavedSearchId.Value)
                .Include(s => s.FloodEvent)
                .FirstOrDefaultAsync(ct);
        } else
        {
            // Use the reference
            existingSearch = await context.SavedSearches.AsNoTracking()
                .Where(s => s.Reference == reference)
                .Include(s => s.FloodEvent)
                .FirstOrDefaultAsync(ct);
        }

        if (existingSearch == null)
        {
            return null;
        }

        return existingSearch;
    }

    public async Task<SavedSearch?> CreateOrUpdateFloodEvent(Guid SavedSearchId, FloodEvent floodEvent, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        SavedSearch? existingSearch;

        // Get the saved search
        if (SavedSearchId == Guid.Empty)
        {
            return null;
        }

        existingSearch = await context.SavedSearches
                .Where(s => s.Id == SavedSearchId)
                .Include (s => s.FloodEvent)
                .FirstOrDefaultAsync(ct);

        if (existingSearch == null)
        {
            return null;
        }

        if (existingSearch.FloodEvent == null)
        {
            context.FloodEvents.Add(floodEvent);
            existingSearch.FloodEvent = floodEvent;
        } else
        {
            // Update existing flood event with new values (except Id)
            var existingEvent = existingSearch.FloodEvent;
            existingSearch.FloodEvent = existingEvent with
            {
                Id = existingEvent.Id, 
                MitigationTaken = floodEvent.MitigationTaken,
                RecordStatus = floodEvent.RecordStatus,
                FloodStatus = floodEvent.FloodStatus,
                InvestigationStatus = floodEvent.InvestigationStatus,
                EstimatedVulnerable = floodEvent.EstimatedVulnerable,
                EstimatedUninhabitable = floodEvent.EstimatedUninhabitable,
                EstimatedNoElectric = floodEvent.EstimatedNoElectric,
                EstimatedNoGas = floodEvent.EstimatedNoGas,
                EstimatedNoFoul = floodEvent.EstimatedNoFoul,
                EstimatedNoWaterSupply = floodEvent.EstimatedNoWaterSupply,
                EstimatedDisruptedUtilities = floodEvent.EstimatedDisruptedUtilities,
                EstimatedResidentialAffected = floodEvent.EstimatedResidentialAffected,
                EstimatedResidentialFlooded = floodEvent.EstimatedResidentialFlooded,
                EstimatedResidentialOther = floodEvent.EstimatedResidentialOther,
                EstimatedCommercialAffected = floodEvent.EstimatedCommercialAffected,
                EstimatedCommercialFlooded = floodEvent.EstimatedCommercialFlooded,
                EstimatedCommercialOther = floodEvent.EstimatedCommercialOther,
                EstimatedTotalExternalOnly = floodEvent.EstimatedTotalExternalOnly,
                EstimatedTotalInternallyFlooded = floodEvent.EstimatedTotalInternallyFlooded,
                StatusNote = floodEvent.StatusNote,
                FloodExtentConfidence = floodEvent.FloodExtentConfidence
            };
        }

        await context.SaveChangesAsync(ct);

        return existingSearch;
    }

    public async Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ReportStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);
    }

    public async Task<IList<EventStatus>> GetEventStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EventStatuses
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
