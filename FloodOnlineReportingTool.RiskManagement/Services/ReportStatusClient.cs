using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.Identity.Abstractions;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public sealed class ReportStatusClient(ILogger<ReportStatusClient> logger, IDownstreamApi downstreamApi) : IReportStatusClient
{
    // TODO: CHECK ClaimsPrincipal? user = default
    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood locations: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var pagedResults = await downstreamApi
            .CallApiForUserAsync<PagedResult<SearchResultFloodLocation>>(RiskManagementSettings.ReportStatusApi, options =>
            {
                options.RelativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations?pageNumber={pageNumber}&pageSize={pageSize}");
            }, cancellationToken: ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood locations found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood locations", pagedResults.TotalCount);
        return pagedResults;

        //    return new()
        //    {
        //        State = totalCount > 0 ? SearchState.ResultsFound : SearchState.NoResults,
        //        Results = pagedResults,
        //    };
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex, "Error occurred while searching flood locations");
        //    return new()
        //    {
        //        State = SearchState.Error,
        //        Errors = [
        //            "There was a problem sarching for flood locations. Please try again",
        //            ex.Message,
        //        ],
        //    };
        //}
    }

    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodProperty>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodProperty>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
