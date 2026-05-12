using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class FloodEventRepository(IDbContextFactory<RiskManagementDbContext> contextFactory) : IFloodEventRepository
{
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

    public async Task<IList<EventStatus>> GetEventStatuses(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EventStatuses
            .AsNoTracking()
            .OrderBy(o => o.Order)
            .ToListAsync(ct);
    }

    // TODO: Add filter by user functions. This should include filtering by:
    // - Organisation of the user
    // - User range assigned to the user
    // - The users permission level (e.g. only show actions that the user has permissions to fulfil)

    public async Task<int> GetEventCountForUser(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // TODO insert user filter
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        return await context.FloodEvents
            .Where(fe => fe.CreatedAt >= thirtyDaysAgo && fe.CreatedAt <= DateTimeOffset.UtcNow)
            .CountAsync(ct);
    }

}
