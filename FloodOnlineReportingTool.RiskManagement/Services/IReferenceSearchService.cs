using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

public interface IReferenceSearchService
{
    /// <summary>
    /// 
    /// </summary>
    Task<GetResult<SearchRedirect>> FindByReference(string reference, CancellationToken ct);
}