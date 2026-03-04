using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchParishNameAsync(string searchTerm, CancellationToken ct);

    Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchWardNameAsync(string searchTerm, CancellationToken ct);
}