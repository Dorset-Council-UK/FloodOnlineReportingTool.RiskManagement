using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

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
    /// Does the flood event exist using the ID
    /// </summary>
    Task<bool> Exists(Guid floodEventId, CancellationToken cancellationToken);

    /// <summary>
    /// Does the flood event search exist using the reference
    /// </summary>
    Task<bool> Exists(string reference, CancellationToken cancellationToken);

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

    /// <summary>
    /// Update the flood event using the reference
    /// </summary>
    /// <returns>A result pattern with the updated flood event, or a list of errors.</returns>
    Task<Result<FloodEvent?>> Update(string reference, FloodEvent floodEvent, CancellationToken cancellationToken);

    Task<IList<EventStatus>> GetEventStatuses(CancellationToken cancellationToken);
}
