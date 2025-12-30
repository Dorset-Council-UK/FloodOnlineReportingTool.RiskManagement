using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services;

/// <summary>
/// Report Status API client using Downstream API
/// </summary>
public interface IReportStatusClient
{
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<PagedResult<SearchResultFloodProperty>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodProperty>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<PagedResult<SearchResultFloodReport>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
}
