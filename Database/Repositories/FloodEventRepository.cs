using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Migrations;
using FloodOnlineReportingTool.Database.Models.FloodEvents;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;

namespace FloodOnlineReportingTool.Database.Repositories;

public class FloodEventRepository(
    IDbContextFactory<RiskManagementDbContext> contextFactory,
    IEventLogRepository eventLogRepository,
    ISavedSearchRepository savedSearchRepository
) : IFloodEventRepository
{
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

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

        var linkedSavedSearch = await savedSearchRepository.Get(floodEvent.SavedSearchId, cancellationToken);
        if (linkedSavedSearch is not null
            && linkedSavedSearch.ResultsFrom is not null
            && linkedSavedSearch.ResultsTo is not null
            && linkedSavedSearch.FloodSources.Count > 0)
        {
            context.FloodEvents.Add(floodEvent);

            // Send a message to trigger the FloodEventCreated message
            var outMessage = new FloodEventCreated(
            linkedSavedSearch.Reference,
            linkedSavedSearch.SearchTitle,
            floodEvent.RecordStatus,
            floodEvent.FloodStatus,
            floodEvent.InvestigationStatus,
            linkedSavedSearch.ResultsFrom.Value,
            linkedSavedSearch.ResultsTo.Value,
            [.. linkedSavedSearch.FloodSources],
            [.. linkedSavedSearch.LimitByOrganisation],
            [.. linkedSavedSearch.LimitByStatus],
            []
            );
            OutboxMessage outboxMessage = new()
            {
                MessageType = nameof(FloodEventCreated),
                Message = JsonSerializer.Serialize(outMessage, _jsonOptions),
                Status = MessageStatus.Pending,
            };
            context.OutboxMessages.Add(outboxMessage);
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
        }
        else
        {
            return Result<FloodEvent?>.Failure([$"A flood event must have a start / end date and at least one source of flooding."]);
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
        FloodEvent? updatedFloodEvent = null;
        var linkedSavedSearch = await savedSearchRepository.Get(floodEvent.SavedSearchId, cancellationToken);
        if (linkedSavedSearch is not null
            && linkedSavedSearch.ResultsFrom is not null
            && linkedSavedSearch.ResultsTo is not null
            && linkedSavedSearch.FloodSources.Count > 0
            )
        {
            updatedFloodEvent = existingFloodEvent with
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

            var investigationStatusChanged = existingFloodEvent.InvestigationStatus != floodEvent.InvestigationStatus;
            context.FloodEvents.Update(updatedFloodEvent);

            // Send a message to trigger the FloodEventUpdated message
            var outMessage = new FloodEventUpdated(
                linkedSavedSearch.Reference,
                linkedSavedSearch.SearchTitle,
                floodEvent.RecordStatus,
                floodEvent.FloodStatus,
                floodEvent.InvestigationStatus,
                linkedSavedSearch.ResultsFrom.Value,
                linkedSavedSearch.ResultsTo.Value,
                [.. linkedSavedSearch.FloodSources],
                [.. linkedSavedSearch.LimitByOrganisation],
                [.. linkedSavedSearch.LimitByStatus],
                [],
                null
             );
            OutboxMessage outboxMessage = new()
            {
                MessageType = nameof(FloodEventUpdated),
                Message = JsonSerializer.Serialize(outMessage, _jsonOptions),
                Status = MessageStatus.Pending,
            };
            context.OutboxMessages.Add(outboxMessage);
            await context.SaveChangesAsync(cancellationToken);

            if (investigationStatusChanged && floodEvent.InvestigationStatus == Section19StatusIds.Section19Required)
            {
                var createEventLogResult = await eventLogRepository.CreateEventLogForFloodEvent(
                    floodEventId,
                    Models.Enums.ActionEventTypes.TriggerInvestigation,
                    cancellationToken);

                if (!createEventLogResult.IsSuccess)
                {
                    return Result<FloodEvent?>.Failure(createEventLogResult.Errors);
                }
            }
        }
        else
        {
            return Result<FloodEvent?>.Failure([$"A flood event must have a start / end date and at least one source of flooding."]);
        }

        // update event log?
        return Result<FloodEvent?>.Success(updatedFloodEvent);
    }

    public async Task<bool> UpdateRecordStatus(Guid floodEventId, Guid Status, CancellationToken cancellationToken) 
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent != null)
        {
            existingFloodEvent.RecordStatus = Status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateFloodStatus(Guid floodEventId, Guid Status, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent != null)
        {
            existingFloodEvent.FloodStatus = Status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateInvestigationStatus(Guid floodEventId, Guid Status, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent != null)
        {
            var investigationStatusChanged = existingFloodEvent.InvestigationStatus != Status;
            existingFloodEvent.InvestigationStatus = Status;

            var linkedSavedSearch = await savedSearchRepository.Get(existingFloodEvent.SavedSearchId, cancellationToken);
            if (investigationStatusChanged
                && linkedSavedSearch is not null
                && linkedSavedSearch.ResultsFrom is not null
                && linkedSavedSearch.ResultsTo is not null
                && linkedSavedSearch.FloodSources.Count > 0)
            {
                var outMessage = new FloodEventUpdated(
                    linkedSavedSearch.Reference,
                    linkedSavedSearch.SearchTitle,
                    existingFloodEvent.RecordStatus,
                    existingFloodEvent.FloodStatus,
                    Status,
                    linkedSavedSearch.ResultsFrom.Value,
                    linkedSavedSearch.ResultsTo.Value,
                    [.. linkedSavedSearch.FloodSources],
                    [.. linkedSavedSearch.LimitByOrganisation],
                    [.. linkedSavedSearch.LimitByStatus],
                    [],
                    null);

                context.OutboxMessages.Add(new OutboxMessage
                {
                    MessageType = nameof(FloodEventUpdated),
                    Message = JsonSerializer.Serialize(outMessage, _jsonOptions),
                    Status = MessageStatus.Pending,
                });
            }

            await context.SaveChangesAsync(cancellationToken);

            if (investigationStatusChanged && Status == Section19StatusIds.Section19Required)
            {
                var createEventLogResult = await eventLogRepository.CreateEventLogForFloodEvent(
                    floodEventId,
                    Models.Enums.ActionEventTypes.TriggerInvestigation,
                    cancellationToken);

                return createEventLogResult.IsSuccess;
            }

            return true;
        }

        return false;
    }

    public async Task<(bool IsSuccess, string? MediaReference)> CreateMediaUrl(Guid floodEventId, DateTimeOffset mediaExpiry, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent is null)
        {
            return (false, null);
        }

        if (!string.IsNullOrEmpty(existingFloodEvent.MediaReference))
        {
            return (false, existingFloodEvent.MediaReference);
        }

        existingFloodEvent.MediaReference = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToUpperInvariant();
        existingFloodEvent.MediaReferenceExpiration = mediaExpiry;
        existingFloodEvent.MediaUploadLock = false;

        await context.SaveChangesAsync(cancellationToken);

        return (true, existingFloodEvent.MediaReference);
    }

    /// <summary>
    /// Sets whether public media upload is locked for the flood event.
    /// </summary>
    public async Task<bool> SetMediaUploadLock(Guid floodEventId, bool isLocked, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent is null || string.IsNullOrWhiteSpace(existingFloodEvent.MediaReference))
        {
            return false;
        }

        existingFloodEvent.MediaUploadLock = isLocked;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Updates the public media URL expiry for the flood event.
    /// </summary>
    public async Task<bool> UpdateMediaUrlExpiry(Guid floodEventId, DateTimeOffset mediaExpiry, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingFloodEvent = await context.FloodEvents
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(fe => fe.Id == floodEventId, cancellationToken);

        if (existingFloodEvent is null || string.IsNullOrWhiteSpace(existingFloodEvent.MediaReference))
        {
            return false;
        }

        existingFloodEvent.MediaReferenceExpiration = mediaExpiry;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IList<EventStatus>> GetEventStatuses(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.EventStatuses
            .AsNoTracking()
            .OrderBy(es => es.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<GetResult<FloodEvent>> GetBySearchReference(
        string searchReference,
        int targetSrid,
        CancellationToken cancellationToken
        )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var floodEvent = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(e => e.MediaItems)
            .Include(e => e.SavedSearch)
            .Where(e => e.SavedSearch.Reference == searchReference)
            .FirstOrDefaultAsync(cancellationToken);

        if (floodEvent is null)
        {
            return GetResult<FloodEvent>.Failure([$"No flood event found with search reference '{searchReference}'."]);
        }

        // Transform geometry if a different SRID is requested
        if (targetSrid != 27700 && floodEvent.SavedSearch.Geom is not null)
        {
            var transformedGeom = await GetTransformedGeometry(
                floodEvent.SavedSearchId,
                targetSrid,
                cancellationToken);

            floodEvent = floodEvent with
            {
                SavedSearch = floodEvent.SavedSearch with
                {
                    Geom = transformedGeom
                }
            };
        }

        return GetResult<FloodEvent>.Success(floodEvent);
    }

    public async Task<FloodEvent> GetFloodEventById(Guid floodEventId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var floodEvent = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(e => e.SavedSearch)
            .Include(e => e.MediaItems)
            .FirstOrDefaultAsync(e => e.Id == floodEventId, cancellationToken);
        if (floodEvent is null)
        {
            throw new InvalidOperationException($"No flood event found with ID '{floodEventId}'.");
        }
        return floodEvent;
    }

    public async Task<FloodEvent?> GetFloodEventByMediaUploadReference(string mediaUploadReference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var floodEvent = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(e => e.MediaReference == mediaUploadReference, cancellationToken);
        return floodEvent;
    }

    private async Task<NetTopologySuite.Geometries.MultiPolygon?> GetTransformedGeometry(
        Guid savedSearchId,
        int targetSrid,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = """
        SELECT ST_Multi(ST_Transform("Geom", @targetSrid))
        FROM "SavedSearches"
        WHERE "Id" = @savedSearchId
        """;

        var savedSearchIdParameter = command.CreateParameter();
        savedSearchIdParameter.ParameterName = "savedSearchId";
        savedSearchIdParameter.Value = savedSearchId;
        command.Parameters.Add(savedSearchIdParameter);

        var targetSridParameter = command.CreateParameter();
        targetSridParameter.ParameterName = "targetSrid";
        targetSridParameter.Value = targetSrid;
        command.Parameters.Add(targetSridParameter);

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result as NetTopologySuite.Geometries.MultiPolygon;
    }

    public async Task<List<FloodEvent>> GetFloodEventsFromPast30Days(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(fe => fe.SavedSearch)
            .Where(fe => fe.CreatedAt >= now.AddDays(-30) && fe.CreatedAt <= now)
            .OrderByDescending(fe => fe.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
