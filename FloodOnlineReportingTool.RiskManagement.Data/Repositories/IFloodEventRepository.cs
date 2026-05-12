using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface IFloodEventRepository
{
    Task<SavedSearch?> CreateOrUpdateFloodEvent(Guid savedSearchId, FloodEvent floodEvent, CancellationToken ct);
    Task<IList<EventStatus>> GetEventStatuses(CancellationToken ct);
    Task<int> GetEventCountForUser(CancellationToken ct);
}
