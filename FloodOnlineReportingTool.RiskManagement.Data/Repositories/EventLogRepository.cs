using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Messaging;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class EventLogRepository(
    IDbContextFactory<RiskManagementDbContext> contextFactory
    ) : IEventLogRepository
{
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task<CreateOrUpdateResult<EventLog>> CreateEventLog(Guid SearchedSearchId, ActionEventTypes EventLogType, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var savedSearch = context.SavedSearches.Where(ss => ss.Id == SearchedSearchId).AsNoTracking().SingleOrDefault();

        if (savedSearch is null)
        {
            return CreateOrUpdateResult<EventLog>.Failure(["Saved search not found"]);
        }
        var floodEventId = savedSearch.FloodEventId;

        var existingRecord = context.EventLogs.Where(el => el.EventId == floodEventId && el.EventAction == EventLogType).AsNoTracking().SingleOrDefault();

        if (existingRecord is not null)
        {
            return CreateOrUpdateResult<EventLog>.Failure(["Already has a record, don't add a second"]);
        }

        var floodEvent = context.FloodEvents.Where(fe => fe.Id == floodEventId).AsNoTracking().SingleOrDefault();
        if (floodEvent is null)
        {
            return CreateOrUpdateResult<EventLog>.Failure(["Flood event not found"]);
        }

        // TODO: add the user requesting to the log
        var newAction = new EventLog
        {
            Id = Guid.NewGuid(),
            EventId = floodEvent.Id,
            FloodEvent = floodEvent,

            LoggedAt = DateTimeOffset.UtcNow,
            ActionedRequestByUserId = null,
            EventAction = EventLogType
        };

        switch (EventLogType)
        {
            case ActionEventTypes.TriggerInvestigation:
                // Send a message to trigger the investigation
                var outMessage = new InvestigationTriggered(
                    floodEvent.Id, 
                    floodEvent.Reference, 
                    DateTimeOffset.UtcNow);
                OutboxMessage outboxMessage = new()
                {
                    MessageType = nameof(InvestigationTriggered),
                    Message = JsonSerializer.Serialize(outMessage, _jsonOptions),
                    Status = MessageStatus.Pending,
                };
                context.OutboxMessages.Add(outboxMessage);
                break;
            default:
                break;
        }
        await context.SaveChangesAsync(ct);

        context.EventLogs.Add(newAction);
        await context.SaveChangesAsync(ct);

        return CreateOrUpdateResult<EventLog>.Success(newAction);
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

        return eventLog is not null ? GetResult<EventLog>.Success(eventLog) : GetResult<EventLog>.Failure(new[] { "Event log not found" });
    }
}
