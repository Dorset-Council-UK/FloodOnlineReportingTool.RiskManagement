using GdsBlazorComponents;
using NetTopologySuite.Geometries;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IGeoServerClient
{
    Task<IReadOnlyCollection<string>> SearchNameAsync(
        string apiSource, 
        string searchTerm,
        ClaimsPrincipal? user,
        CancellationToken ct);
        
    Task<Geometry?> GetGeometryFromSourceBySearch(
        string apiSource, 
        string searchTerm,
        ClaimsPrincipal? user,
        CancellationToken ct);
}