using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IReferenceSearchService
{
    Task<SearchRedirect?> FindByReference(string reference, CancellationToken ct);
}