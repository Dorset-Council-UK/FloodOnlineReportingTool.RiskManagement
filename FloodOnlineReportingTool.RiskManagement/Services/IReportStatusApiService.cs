using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IReportStatusApiService
{
    Task<SearchResult> SearchUprn(long uprn, CancellationToken cancellationToken = default);
}