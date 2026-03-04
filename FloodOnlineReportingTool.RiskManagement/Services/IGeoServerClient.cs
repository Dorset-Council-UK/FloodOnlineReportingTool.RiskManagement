using GdsBlazorComponents;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchNameAsync(string apiSource, string searchTerm, CancellationToken ct);

    Task<Geometry?> GetGeometryFromSourceBySearch(string apiSource, string searchTerm, CancellationToken ct);
}