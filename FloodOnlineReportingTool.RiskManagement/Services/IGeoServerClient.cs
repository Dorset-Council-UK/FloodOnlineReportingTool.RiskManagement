using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchNameAsync(string apiSource, string searchTerm, CancellationToken ct);

    Task<string?> GetGeometryFromSourceBySearch(string apiSource, string searchTerm, CancellationToken ct);
}