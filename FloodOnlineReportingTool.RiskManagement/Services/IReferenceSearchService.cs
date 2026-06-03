using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Services
{
    public interface IReferenceSearchService
    {
        /// <summary>
        /// 
        /// </summary>
        Task<GetResult<SearchRedirect>> FindByReference(string reference, CancellationToken ct);
    }
}