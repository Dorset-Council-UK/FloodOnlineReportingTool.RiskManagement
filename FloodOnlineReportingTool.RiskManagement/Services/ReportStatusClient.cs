using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.Geometries;
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

    private async Task<PagedResult<T>?> GetSearchResults<T>(string relativePath, CancellationToken ct)
    {
        return await downstreamApi
            .CallApiForUserAsync<PagedResult<T>>(RiskManagementOptions.ReportStatusApi, options =>
            {
                options.RelativePath = relativePath;
                options.Deserializer = (content) =>
                {
                    if (content is null) return null;
                    var stream = content.ReadAsStream(ct);
                    return JsonSerializer.Deserialize<PagedResult<T>>(stream, _jsonSerializerOptions);
                };
            }, cancellationToken: ct);
    }

    /// <summary>
    /// Searches the downstream API for a single flood report matching the given reference.
    /// Returns a success result containing the report if found, or a failure result if not.
    /// </summary>
    public async Task<GetResult<SearchResultFloodReport>> SearchFloodReportByReference(string reference, CancellationToken ct = default)
    {
        logger.LogInformation("Searching downstream API for flood report by reference");

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/flood/reports/reference/{reference}?searchType=Report");
        try
        {
            // The endpoint returns a single result or nothing — not a paged collection
            var floodReportResult = await downstreamApi
                .CallApiForUserAsync<SearchResultFloodReport>(RiskManagementOptions.ReportStatusApi, options =>
                {
                    options.RelativePath = relativePath;
                    options.Deserializer = (content) =>
                    {
                        if (content is null) return null;
                        var stream = content.ReadAsStream(ct);
                        return JsonSerializer.Deserialize<SearchResultFloodReport>(stream, _jsonSerializerOptions);
                    };
                }, cancellationToken: ct);

            if (floodReportResult is not null)
            {
                logger.LogInformation("Flood report with reference found in downstream API");
                return GetResult<SearchResultFloodReport>.Success(floodReportResult);
            }

            logger.LogWarning("No flood report found in downstream API");
            return GetResult<SearchResultFloodReport>.Failure(["No flood report found for reference."]);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference — no matching report exists", ex.StatusCode);
            return GetResult<SearchResultFloodReport>.Failure(["No flood report found for reference."]);
        }
    }

    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood locations: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodLocation>(relativePath, ct);

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
        logger.LogInformation("Searching flood locations with flood reports: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations/with-floodreports?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodLocation>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood locations with flood reports found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood locations with flood reports", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood locations by point: PageNumber={PageNumber}, PageSize={PageSize}, Easting={Easting}, Northing={Northing}, Buffer={Buffer}", pageNumber, pageSize, easting, northing, buffer);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodlocations/by-point?pageNumber={pageNumber}&pageSize={pageSize}&easting={easting}&northing={northing}&buffer={buffer}");
        var pagedResults = await GetSearchResults<SearchResultFloodLocation>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood locations by point found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood locations by point", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodProperty>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood properties: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodproperties?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodProperty>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood properties found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood properties", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodProperty>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood properties with flood reports: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodproperties/with-floodreports?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodProperty>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood properties with flood reports found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood properties with flood reports", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood reports: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodreports?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodReport>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood reports within metres: PageNumber={PageNumber}, PageSize={PageSize}, Easting={Easting}, Northing={Northing}, Metres={Metres}", pageNumber, pageSize, easting, northing, metres);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodreports/within-metres?pageNumber={pageNumber}&pageSize={pageSize}&easting={easting}&northing={northing}&metres={metres}");
        var pagedResults = await GetSearchResults<SearchResultFloodReport>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports within metres found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports within metres", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood reports by UPRN: PageNumber={PageNumber}, PageSize={PageSize}, UPRN={UPRN}", pageNumber, pageSize, uprn);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodreports/uprn?pageNumber={pageNumber}&pageSize={pageSize}&uprn={uprn}");
        var pagedResults = await GetSearchResults<SearchResultFloodReport>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports by UPRN found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports by UPRN", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood reports by USRN: PageNumber={PageNumber}, PageSize={PageSize}, USRN={Usrn}", pageNumber, pageSize, usrn);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodreports/usrn?pageNumber={pageNumber}&pageSize={pageSize}&usrn={usrn}");
        var pagedResults = await GetSearchResults<SearchResultFloodReport>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports by USRN");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports by USRN", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood roads: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodroads?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodRoad>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood roads found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood roads", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood roads with flood reports: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodroads/with-floodreports?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await GetSearchResults<SearchResultFloodRoad>(relativePath, ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood roads with flood reports found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood roads with flood reports", pagedResults.TotalCount);
        return pagedResults;
    }

    public async Task<PagedResult<SearchResultFloodReport>> SearchFloodReports(DateTimeOffset? resultsFrom, DateTimeOffset? resultsTo, ICollection<Guid> floodSources, long? uprn, long? usrn, string? postcode, MultiPolygon? geom, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        logger.LogInformation("Searching flood reports: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

        var request = new
        {
            resultsFrom,
            resultsTo,
            floodSources,
            uprn,
            usrn,
            postcode,
            geom
        };

        var relativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/floodreports/search?pageNumber={pageNumber}&pageSize={pageSize}");
        var pagedResults = await downstreamApi.CallApiForUserAsync<object, PagedResult<SearchResultFloodReport>>(
            RiskManagementOptions.ReportStatusApi,
            request,
            options =>
            {
                options.HttpMethod = HttpMethod.Post.Method;
                options.RelativePath = relativePath;
                options.Serializer = (input) => JsonContent.Create(input, options: _jsonSerializerOptions);
                options.Deserializer = (content) =>
                {
                    if (content is null) return null;
                    var stream = content.ReadAsStream(ct);
                    return JsonSerializer.Deserialize<PagedResult<SearchResultFloodReport>>(stream, _jsonSerializerOptions);
                };
            },
            cancellationToken: ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports found");
            return new([], 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports", pagedResults.TotalCount);
        return pagedResults;
    }
}
