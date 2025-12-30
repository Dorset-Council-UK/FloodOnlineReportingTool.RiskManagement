using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.IO.Converters;
using System.Globalization;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public sealed class ReportStatusClient(ILogger<ReportStatusClient> logger, IDownstreamApi downstreamApi) : IReportStatusClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new GeoJsonConverterFactory() },
    };

    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood locations: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var pagedResults = await downstreamApi
            .CallApiForUserAsync<PagedResult<SearchResultFloodLocation>>(RiskManagementSettings.ReportStatusApi, options =>
            {
                options.RelativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations?pageNumber={pageNumber}&pageSize={pageSize}");
                options.Deserializer = (content) =>
                {
                    if (content is null) return null;
                    var stream = content.ReadAsStream(ct);
                    return JsonSerializer.Deserialize<PagedResult<SearchResultFloodLocation>>(stream, _jsonSerializerOptions);
                };
            }, cancellationToken: ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood locations found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood locations", pagedResults.TotalCount);
        return pagedResults;
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
