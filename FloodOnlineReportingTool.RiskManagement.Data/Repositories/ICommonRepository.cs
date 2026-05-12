using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface ICommonRepository
{
    Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct);
    Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct);
    Task<SavedSearch?> GetSavedSearch(string reference, CancellationToken ct);
    Task<SavedSearch?> GetSavedSearch(Guid savedSearchId, CancellationToken ct);
    Task<SavedSearch> GetOrCreateSavedSearch(Guid? id, string? reference, SavedSearch searchCriteria, CancellationToken ct);
    Task<SavedSearch> UpdateSavedSearch(string reference, SavedSearch searchCriteria, CancellationToken ct);
    Task<bool> SetSavedSearchTitle(Guid id, string Title, CancellationToken ct);
    Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct);
    Task<IList<FloodStatus>> GetFloodStatuses(CancellationToken ct);
    Task<IList<InvestigationStatus>> GetInvestigationStatuses(CancellationToken ct);
}
