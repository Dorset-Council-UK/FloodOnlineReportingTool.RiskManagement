using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchParishNameAsync(string searchTerm, CancellationToken ct = default);
}