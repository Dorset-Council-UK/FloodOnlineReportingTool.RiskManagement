namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyList<string>> SearchParishNameAsync(string searchTerm, CancellationToken ct = default);
}