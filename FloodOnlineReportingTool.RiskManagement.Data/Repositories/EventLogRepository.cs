using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Messaging;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class EventLogRepository(
    IDbContextFactory<RiskManagementDbContext> contextFactory
) : IEventLogRepository
{
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task<Result<EventLog>> CreateEventLog(Guid savedSearchId, ActionEventTypes eventLogType, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var savedSearch = await context.SavedSearches
            .AsNoTracking()
            .FirstOrDefaultAsync(ss => ss.Id == savedSearchId, ct);
        if (savedSearch is null)
        {
            return Result<EventLog>.Failure(["Saved search not found"]);
        }

        if (savedSearch.FloodEventId is null || savedSearch.FloodEventId == Guid.Empty)
        {
            return Result<EventLog>.Failure(["Saved search does not have a flood event associated with it"]);
        }

        return await CreateEventLogForFloodEvent(savedSearch.FloodEventId.Value, eventLogType, ct);
    }

    public async Task<Result<EventLog>> CreateEventLogForFloodEvent(Guid floodEventId, ActionEventTypes eventLogType, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var eventlogExists = await context.EventLogs
            .AsNoTracking()
            .AnyAsync(el => el.FloodEventId == floodEventId && el.EventAction == eventLogType, cancellationToken);
        if (eventlogExists)
        {
            return Result<EventLog>.Failure(["Already has an event log, don't add a second"]);
        }

        var floodEvent = await context.FloodEvents.FindAsync([floodEventId], cancellationToken);
        if (floodEvent is null)
        {
            return Result<EventLog>.Failure(["Flood event not found"]);
        }

        // TODO: add the user requesting to the log
        var newAction = new EventLog
        {
            FloodEventId = floodEvent.Id,
            FloodEvent = floodEvent,
            ActionedRequestByUserId = null,
            EventAction = eventLogType,
        };

        switch (eventLogType)
        {
            case ActionEventTypes.TriggerInvestigation:
                // Send a message to trigger the investigation
                var outMessage = new FloodEventS19Triggered(
                    floodEvent.Reference,
                    DateTimeOffset.UtcNow);
                OutboxMessage outboxMessage = new()
                {
                    MessageType = nameof(FloodEventS19Triggered),
                    Message = JsonSerializer.Serialize(outMessage, _jsonOptions),
                    Status = MessageStatus.Pending,
                };
                context.OutboxMessages.Add(outboxMessage);
                break;
            default:
                break;
        }
        context.EventLogs.Add(newAction);
        await context.SaveChangesAsync(cancellationToken);

        return Result<EventLog>.Success(newAction);
    }

    // TODO: Add filter by user functions. This should include filtering by:
    // - Organisation of the user
    // - User range assigned to the user
    // - The users permission level (e.g. only show actions that the user has permissions to fulfil)

    public async Task<GetResult<EventLog>> GetEventLogById(Guid eventLogId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        // TODO insert user filter
        var eventLog = await context.EventLogs
            .Include(el => el.FloodEvent)
            .Where(el => el.Id == eventLogId)
            .FirstOrDefaultAsync(ct);

        return eventLog is not null ? GetResult<EventLog>.Success(eventLog) : GetResult<EventLog>.Failure(["Event log not found"]);
    }
}
