using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IReferenceSearchService
{
    Task<SearchRedirect?> FindByReference(string reference, CancellationToken ct);

    Task<IReadOnlyList<OmniboxSearchResult>> Search(string query, int maxResults, CancellationToken ct);
}