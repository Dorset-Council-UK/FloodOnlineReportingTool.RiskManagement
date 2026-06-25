using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class FloodEventRepository(
    IDbContextFactory<RiskManagementDbContext> contextFactory,
    IEventLogRepository eventLogRepository
) : IFloodEventRepository
{
    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodEvents.CountAsync(cancellationToken);
    }

    public async Task<int> Count(string userId, CancellationToken cancellationToken)
    {
        // TODO: Add filter by user functions. This should include filtering by:
        // - Organisation of the user
        // - User range assigned to the user
        // - The users permission level (e.g. only show actions that the user has permissions to fulfil)

        throw new NotImplementedException();
    }

    public async Task<int> CountCreatedInPast30Days(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodEvents
            .Where(fe => fe.CreatedAt >= now.AddDays(-30) && fe.CreatedAt <= now)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> Exists(Guid floodEventId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(fe => fe.Id == floodEventId, cancellationToken);
    }

    public async Task<Result<FloodEvent?>> Create(FloodEvent floodEvent, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var exists = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(fe => fe.Id == floodEvent.Id, cancellationToken);

        if (exists)
        {
            return Result<FloodEvent?>.Failure([$"A flood event with ID '{floodEvent.Id}' already exists."]);
        }

        context.FloodEvents.Add(floodEvent);
        await context.SaveChangesAsync(cancellationToken);

        if (floodEvent.InvestigationStatus == Section19StatusIds.Section19Required)
        {
            // Trigger an investigation request if not already done.
            var createEventLogResult = await eventLogRepository.CreateEventLogForFloodEvent(floodEvent.Id, Models.Enums.ActionEventTypes.TriggerInvestigation, cancellationToken);
            if (!createEventLogResult.IsSuccess)
            {
                return Result<FloodEvent?>.Failure(createEventLogResult.Errors);
            }
        }

        return Result<FloodEvent?>.Success(floodEvent);
    }

    public async Task<Result<FloodEvent?>> Update(Guid floodEventId, FloodEvent floodEvent, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent is null)
        {
            return Result<FloodEvent?>.Failure([$"No flood event found with ID '{floodEventId}'."]);
        }

        var updatedFloodEvent = existingFloodEvent with
        {
            // Id
            // Reference
            // CreatedAt

            EstimatedCommercialAffected = floodEvent.EstimatedCommercialAffected,
            EstimatedCommercialFlooded = floodEvent.EstimatedCommercialFlooded,
            EstimatedCommercialOther = floodEvent.EstimatedCommercialOther,
            EstimatedDisruptedUtilities = floodEvent.EstimatedDisruptedUtilities,
            EstimatedNoElectric = floodEvent.EstimatedNoElectric,
            EstimatedNoFoul = floodEvent.EstimatedNoFoul,
            EstimatedNoGas = floodEvent.EstimatedNoGas,
            EstimatedNoWaterSupply = floodEvent.EstimatedNoWaterSupply,
            EstimatedResidentialAffected = floodEvent.EstimatedResidentialAffected,
            EstimatedResidentialFlooded = floodEvent.EstimatedResidentialFlooded,
            EstimatedResidentialOther = floodEvent.EstimatedResidentialOther,
            EstimatedTotalExternalOnly = floodEvent.EstimatedTotalExternalOnly,
            EstimatedTotalInternallyFlooded = floodEvent.EstimatedTotalInternallyFlooded,
            EstimatedUninhabitable = floodEvent.EstimatedUninhabitable,
            EstimatedVulnerable = floodEvent.EstimatedVulnerable,
            FloodExtentConfidence = floodEvent.FloodExtentConfidence,
            FloodStatus = floodEvent.FloodStatus,
            InvestigationStatus = floodEvent.InvestigationStatus,
            MitigationTaken = floodEvent.MitigationTaken,
            RecordStatus = floodEvent.RecordStatus,
            StatusNote = floodEvent.StatusNote,
        };

        context.FloodEvents.Update(updatedFloodEvent);
        await context.SaveChangesAsync(cancellationToken);

        // update event log?

        return Result<FloodEvent?>.Success(updatedFloodEvent);
    }

    public async Task<IList<EventStatus>> GetEventStatuses(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.EventStatuses
            .AsNoTracking()
            .OrderBy(es => es.Order)
            .ToListAsync(cancellationToken);
    }
}
