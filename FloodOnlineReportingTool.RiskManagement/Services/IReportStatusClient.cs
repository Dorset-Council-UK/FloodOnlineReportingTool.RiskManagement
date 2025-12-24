using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services;

/// <summary>
/// Report Status API client using Downstream API
/// </summary>
internal interface IReportStatusClient
{
    Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocationsWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<SearchResult<PagedResult<SearchResultFloodProperty>>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodProperty>>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    Task<SearchResult<PagedResult<SearchResultFloodRoad>>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<SearchResult<PagedResult<SearchResultFloodRoad>>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
}
