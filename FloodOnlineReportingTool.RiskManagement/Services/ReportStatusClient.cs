using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Abstractions;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public sealed class ReportStatusClient(ILogger<ReportStatusClient> logger, IDownstreamApi downstreamApi) : IReportStatusClient
{
    // TODO: CHECK ClaimsPrincipal? user = default
    public async Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood locations: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var pagedResults = await downstreamApi
            .CallApiForUserAsync<PagedResult<SearchResultFloodLocation>>(RiskManagementSettings.ReportStatusApi, options =>
            {
                options.RelativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations?pageNumber={pageNumber}&pageSize={pageSize}");
            }, cancellationToken: ct);

        logger.LogInformation("Successfully retrieved flood locations: PageNumber={PageNumber}, PageSize={PageSize}, TotalCount={TotalCount}", pageNumber, pageSize, pagedResults.TotalCount);
        return SearchResult<PagedResult<SearchResultFloodLocation>>.Success(pagedResults);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocationsWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodLocation>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodLocation>>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodLocation>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodProperty>>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodProperty>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodProperty>>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodProperty>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodReport>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodReport>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodReport>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodReport>>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodReport>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodRoad>>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodRoad>>.Failure(["TODO"]);
    }

    public async Task<SearchResult<PagedResult<SearchResultFloodRoad>>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return SearchResult<PagedResult<SearchResultFloodRoad>>.Failure(["TODO"]);
    }
}
