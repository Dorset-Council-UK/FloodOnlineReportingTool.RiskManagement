using FloodOnlineReportingTool.Database.Models.FloodEvents;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IFloodEventRepository
{
    /// <summary>
    /// Count the number of flood events
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of flood events for the user
    /// </summary>
    Task<int> Count(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of flood events created in the past 30 days
    /// </summary>
    Task<int> CountCreatedInPast30Days(CancellationToken cancellationToken);

    /// <summary>
    /// Does the flood event exist using the ID
    /// </summary>
    Task<bool> Exists(Guid floodEventId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a flood event
    /// </summary>
    /// <returns>A result pattern with the new flood event, or a list of errors.</returns>
    Task<Result<FloodEvent?>> Create(FloodEvent floodEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Update the flood event using the ID
    /// </summary>
    /// <returns>A result pattern with the updated flood event, or a list of errors.</returns>
    Task<Result<FloodEvent?>> Update(Guid floodEventId, FloodEvent floodEvent, CancellationToken cancellationToken);

    Task<IList<EventStatus>> GetEventStatuses(CancellationToken cancellationToken);

    Task<GetResult<FloodEvent>> GetBySearchReference(string searchReference, int targetSrid, CancellationToken cancellationToken);

    Task<FloodEvent> GetFloodEventById(Guid floodEventId, CancellationToken cancellationToken);
    Task<FloodEvent?> GetFloodEventByMediaUploadReference(string mediaUploadReference, CancellationToken cancellationToken);

    Task<(bool IsSuccess, string? MediaReference)> CreateMediaUrl(Guid floodEventId, DateTimeOffset mediaExpiry, CancellationToken cancellationToken);

    /// <summary>
    /// Sets whether public media upload is locked for the flood event.
    /// </summary>
    Task<bool> SetMediaUploadLock(Guid floodEventId, bool isLocked, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the public media URL expiry for the flood event.
    /// </summary>
    Task<bool> UpdateMediaUrlExpiry(Guid floodEventId, DateTimeOffset mediaExpiry, CancellationToken cancellationToken);

    Task<List<FloodEvent>> GetFloodEventsFromPast30Days(CancellationToken cancellationToken);
}
