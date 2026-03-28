using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore.Internal;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface ICommonRepository
{
    Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct);
    Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct);
    Task<SavedSearch?> GetSavedSearchByReference(string reference, CancellationToken ct);
    Task<SavedSearch> GetOrCreateSavedSearch(Guid? id, string? reference, SavedSearch searchCriteria, CancellationToken ct);
    Task<FloodEvent?> GetFloodEvent(Guid SavedSearchId, CancellationToken ct);
    Task<SavedSearch?> CreateOrUpdateFloodEvent(Guid SavedSearchId, FloodEvent floodEvent, CancellationToken ct);
    Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct);
    Task<IList<EventStatus>> GetEventStatuses(CancellationToken ct);
    Task<IList<FloodStatus>> GetFloodStatuses(CancellationToken ct);
    Task<IList<InvestigationStatus>> GetInvestigationStatuses(CancellationToken ct);
}
