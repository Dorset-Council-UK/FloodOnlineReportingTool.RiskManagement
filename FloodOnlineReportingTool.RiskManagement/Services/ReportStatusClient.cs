using FloodOnlineReportingTool.Contracts.Shared.Models;
using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Report;
using FloodOnlineReportingTool.RiskManagement.Options;
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
    /// Checks whether a flood report exists for the given reference in the downstream API.
    /// Returns a <see cref="SearchRedirect"/> with the report Id if found, used for redirect
    /// routing only — not for display purposes.
    /// </summary>
    public async Task<SearchRedirect?> FindFloodReportByReference(string reference, string searchType = "Report", CancellationToken ct = default)
    {
        logger.LogInformation("Checking downstream API for flood report existence by reference");

        var relativePath = $"/api/flood/reports/reference/{Uri.EscapeDataString(reference)}?searchType={searchType}";
        try
        {
            // The endpoint returns a single result or nothing — not a paged collection
            var floodReportResult = await downstreamApi
                .CallApiForUserAsync<FloodReportWithSourceReferences>(RiskManagementOptions.ReportStatusApi, options =>
                {
                    options.RelativePath = relativePath;
                    options.Deserializer = (content) =>
                    {
                        if (content is null) return null;
                        var stream = content.ReadAsStream(ct);
                        return JsonSerializer.Deserialize<FloodReportWithSourceReferences>(stream, _jsonSerializerOptions);
                    };
                }, cancellationToken: ct);

            if (floodReportResult is not null)
            {
                logger.LogInformation("Flood report with reference found in downstream API");
                return new SearchRedirect
                {
                    Id = floodReportResult.Id,
                    Reference = floodReportResult.Reference,
                    RedirectType = RecordTypes.Report,
                };
            }

            logger.LogWarning("No flood report found in downstream API");
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference — no matching report exists", ex.StatusCode);
            return null;
        }
    }

    // Retrieves the flood report and resolves all source references into the slim SearchResultFloodReportSource
    // contract type. Used by GetFloodReportDetail where the full entity fields are not required.
    private async Task<(GetResult<FloodReportWithSourceReferences> _report, List<SearchResultFloodReportSource> _sources)> GetFloodReportResults(
        string reference,
        bool firstSourceOnly = false,
        CancellationToken ct = default)
    {
        var searchResult = await SearchFloodReportByReference(reference, ct);

        if (!searchResult.IsSuccess)
        {
            logger.LogWarning("Flood report not found for reference");
            return (GetResult<FloodReportWithSourceReferences>.Failure(["Flood report not found."]), []);
        }

        var floodReport = searchResult.ResultModel;

        if (floodReport is null)
        {
            return (GetResult<FloodReportWithSourceReferences>.Failure(["Flood report result was empty."]), []);
        }

        // Resolve each source reference string into a slim SearchResultFloodReportSource.
        // The API returns references rather than full source objects to avoid circular references.
        var sourceRecords = new List<SearchResultFloodReportSource>(floodReport.SourceReferences.Count);

        foreach (var sourceReference in floodReport.SourceReferences)
        {
            var sourceResult = await SearchFloodReportSourceByReference(floodReport.Id, sourceReference, ct);

            if (!sourceResult.IsSuccess || sourceResult.ResultModel is null)
            {
                logger.LogWarning("Flood report source not found for reference {SourceReference}", sourceReference);
                return (GetResult<FloodReportWithSourceReferences>.Failure([$"Flood report source not found for reference '{sourceReference}'."]), []);
            }

            sourceRecords.Add(sourceResult.ResultModel);
            if (firstSourceOnly)
            {
                logger.LogInformation("FirstSourceOnly flag is set - only resolving the first source reference");
                break;
            }
        }

        return (GetResult<FloodReportWithSourceReferences>.Success(floodReport), sourceRecords);
    }

    // Retrieves the flood report and resolves all source references into the full FloodReportSource
    // entity. Used by GetFloodReportOverview where all fields (LocationDescription, Geom, ContactRecordTypes, etc.) are required.
    private async Task<(GetResult<FloodReportWithSourceReferences> _report, List<FloodReportSource> _sources)> GetFullFloodReportResults(
        string reference,
        CancellationToken ct = default)
    {
        var searchResult = await SearchFloodReportByReference(reference, ct);

        if (!searchResult.IsSuccess)
        {
            logger.LogWarning("Flood report not found for reference");
            return (GetResult<FloodReportWithSourceReferences>.Failure(["Flood report not found."]), []);
        }

        var floodReport = searchResult.ResultModel;

        if (floodReport is null)
        {
            return (GetResult<FloodReportWithSourceReferences>.Failure(["Flood report result was empty."]), []);
        }

        var sourceRecords = new List<FloodReportSource>(floodReport.SourceReferences.Count);

        foreach (var sourceReference in floodReport.SourceReferences)
        {
            var sourceResult = await GetFullFloodReportSourceByReference(floodReport.Id, sourceReference, ct);

            if (!sourceResult.IsSuccess || sourceResult.ResultModel is null)
            {
                logger.LogWarning("Flood report source not found for reference {SourceReference}", sourceReference);
                return (GetResult<FloodReportWithSourceReferences>.Failure([$"Flood report source not found for reference '{sourceReference}'."]), []);
            }

            sourceRecords.Add(sourceResult.ResultModel);
        }

        return (GetResult<FloodReportWithSourceReferences>.Success(floodReport), sourceRecords);
    }

    // Returns the full FloodReportSource entity including all fields.
    // Used when the caller needs fields not present on SearchResultFloodReportSource
    // (e.g., LocationDescription, Geom, ContactRecordTypes).
    private async Task<GetResult<FloodReportSource>> GetFullFloodReportSourceByReference(Guid floodReportId, string reference, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching full flood report source from downstream API by reference");

        var relativePath = $"/api/flood/report/{floodReportId}/sources/{Uri.EscapeDataString(reference)}";
        try
        {
            var result = await downstreamApi
                .CallApiForUserAsync<FloodReportSource>(RiskManagementOptions.ReportStatusApi, options =>
                {
                    options.RelativePath = relativePath;
                    options.Deserializer = (content) =>
                    {
                        if (content is null) return null;
                        var stream = content.ReadAsStream(ct);
                        return JsonSerializer.Deserialize<FloodReportSource>(stream, _jsonSerializerOptions);
                    };
                }, cancellationToken: ct);

            if (result is not null)
            {
                logger.LogInformation("Full flood report source found in downstream API");
                return GetResult<FloodReportSource>.Success(result);
            }

            logger.LogWarning("No flood report source found in downstream API");
            return GetResult<FloodReportSource>.Failure(["No flood report source found for reference."]);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference — no matching report source exists", ex.StatusCode);
            return GetResult<FloodReportSource>.Failure(["No flood report source found for reference."]);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Downstream API request failed for flood report source — {Message}", ex.Message);
            return GetResult<FloodReportSource>.Failure(["Downstream API request failed for flood report source."]);
        }
    }

    public async Task<GetResult<ReportOverviewViewModel>> GetFloodReportOverview(string reference, CancellationToken ct = default)
    {
        logger.LogInformation("Retrieving full flood report overview for reference");

        // Use the full source variant to retain all fields (Geom, LocationDescription, ContactRecordTypes, etc.)
        var searchResult = await GetFullFloodReportResults(reference, ct);

        if (!searchResult._report.IsSuccess)
        {
            var errorMessage = searchResult._report.Errors.FirstOrDefault() ?? "Flood report not found.";
            return GetResult<ReportOverviewViewModel>.Failure([errorMessage]);
        }

        var floodReport = searchResult._report.ResultModel;
        if (floodReport is null)
        {
            return GetResult<ReportOverviewViewModel>.Failure(["Flood report result was empty."]);
        }

        var sourceRecords = searchResult._sources
            .Select(source => new ReportOverviewSourceItem
            {
                Reference = source.Reference,
                ViewUri = source.ViewUri,
                CreatedUtc = source.CreatedUtc,
                UpdatedUtc = source.UpdatedUtc,
                Uprn = source.Uprn,
                Usrn = source.Usrn,
                Easting = source.Easting,
                Northing = source.Northing,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                ImpactStartUtc = source.ImpactStartUtc,
                ImpactDurationHours = source.ImpactDurationHours,
                IsOnGoing = source.IsOnGoing,
                IsUninhabitable = source.IsUninhabitable,
                VulnerableCount = source.VulnerableCount,
                HasInvestigation = source.HasInvestigation,
                HasContacts = source.HasContacts,
                ContactRecordTypes = source.ContactRecordTypes.Select(c => c.ToString()).ToArray(),
                FloodCauses = source.FloodCauses.Select(fc => fc.Name).ToArray(),
                Organisations = source.Organisations.Select(o => o.OrganisationName).ToArray(),
            })
            .ToArray();

        // Use the first source's geometry and location for the parent view model,
        // as these represent the primary flood location for display on the page.
        var firstSource = searchResult._sources.FirstOrDefault();
        if (firstSource is null)
        {
            return GetResult<ReportOverviewViewModel>.Failure(["Flood report has no source records."]);
        }

        var returnModel = new ReportOverviewViewModel
        {
            Id = floodReport.Id,
            CreatedUtc = floodReport.CreatedUtc,
            Reference = floodReport.Reference,
            UpdatedAt = floodReport.UpdatedUtc,
            UpdatedBy = null,
            RecordStatusId = floodReport.RecordStatus,
            Geom = firstSource.Geom,
            LocationDescription = firstSource.LocationDescription ?? string.Empty,
            SourceRecords = sourceRecords,
            Latitude = firstSource.Latitude,
            Longitude = firstSource.Longitude,
        };

        return GetResult<ReportOverviewViewModel>.Success(returnModel);
    }

    /// <summary>
    /// Retrieves the full flood report detail for display on the details page.
    /// The downstream API avoids circular references by returning source data as string references
    /// on <see cref="FloodReportWithSourceReferences"/>. This method resolves each reference via
    /// a second API call, then assembles the full <see cref="SearchResultFloodReport"/> for the view.
    /// </summary>
    public async Task<GetResult<SearchResultFloodReport>> GetFloodReportDetail(string reference, bool firstSourceOnly = false, CancellationToken ct = default)
    {
        logger.LogInformation("Retrieving full flood report detail for reference");

        var searchResult = await GetFloodReportResults(reference, firstSourceOnly, ct);

        if (!searchResult._report.IsSuccess)
        {
            logger.LogWarning("Flood report not found for reference");
            return GetResult<SearchResultFloodReport>.Failure(["Flood report not found."]);
        }

        var floodReport = searchResult._report.ResultModel;
        var sourceRecords = searchResult._sources;

        // Map FloodReportWithSourceReferences + resolved sources → SearchResultFloodReport.
        // RecordStatus (Guid) on the parent maps directly to RecordStatusId on the result.
        var result = new SearchResultFloodReport(
            Id: floodReport.Id,
            Reference: floodReport.Reference,
            RecordStatusId: floodReport.RecordStatus,
            SourceRecords: sourceRecords
        );

        logger.LogInformation("Successfully assembled flood report detail with {SourceCount} source records", sourceRecords.Count);
        return GetResult<SearchResultFloodReport>.Success(result);
    }

    /// <summary>
    /// Searches the downstream API for a single flood report matching the given reference.
    /// Returns the raw <see cref="FloodReportWithSourceReferences"/> shape — source records
    /// are excluded by the API to avoid circular references and are returned as references instead.
    /// </summary>
    private async Task<GetResult<FloodReportWithSourceReferences>> SearchFloodReportByReference(string reference, CancellationToken ct = default)
    {
        logger.LogInformation("Searching downstream API for flood report by reference");

        var relativePath = $"/api/flood/reports/reference/{Uri.EscapeDataString(reference)}?searchType=Report";
        try
        {
            // The endpoint returns a single result or nothing — not a paged collection
            var floodReportResult = await downstreamApi
                .CallApiForUserAsync<FloodReportWithSourceReferences>(RiskManagementOptions.ReportStatusApi, options =>
                {
                    options.RelativePath = relativePath;
                    options.Deserializer = (content) =>
                    {
                        if (content is null) return null;
                        var stream = content.ReadAsStream(ct);
                        return JsonSerializer.Deserialize<FloodReportWithSourceReferences>(stream, _jsonSerializerOptions);
                    };
                }, cancellationToken: ct);

            if (floodReportResult is not null)
            {
                logger.LogInformation("Flood report with reference found in downstream API");
                return GetResult<FloodReportWithSourceReferences>.Success(floodReportResult);
            }

            logger.LogWarning("No flood report found in downstream API");
            return GetResult<FloodReportWithSourceReferences>.Failure(["No flood report found for reference."]);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference — no matching report exists", ex.StatusCode);
            return GetResult<FloodReportWithSourceReferences>.Failure(["No flood report found for reference."]);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Downstream API request failed for flood report source — {Message}", ex.Message);
            return GetResult<FloodReportWithSourceReferences>.Failure(["Downstream API request failed for flood report source."]);
        }
    }

    private async Task<GetResult<SearchResultFloodReportSource>> SearchFloodReportSourceByReference(Guid floodReportId, string reference, CancellationToken ct = default)
    {
        logger.LogInformation("Searching downstream API for flood report source by reference");

        var relativePath = $"/api/flood/report/{floodReportId}/sources/{Uri.EscapeDataString(reference)}";
        try
        {
            // The endpoint returns a single result or nothing — not a paged collection
            var floodReportResult = await downstreamApi
                .CallApiForUserAsync<SearchResultFloodReportSource>(RiskManagementOptions.ReportStatusApi, options =>
                {
                    options.RelativePath = relativePath;
                    options.Deserializer = (content) =>
                    {
                        if (content is null) return null;
                        var stream = content.ReadAsStream(ct);
                        return JsonSerializer.Deserialize<SearchResultFloodReportSource>(stream, _jsonSerializerOptions);
                    };
                }, cancellationToken: ct);

            if (floodReportResult is not null)
            {
                logger.LogInformation("Flood report source with reference found in downstream API");
                return GetResult<SearchResultFloodReportSource>.Success(floodReportResult);
            }

            logger.LogWarning("No flood report source found in downstream API");
            return GetResult<SearchResultFloodReportSource>.Failure(["No flood report source found for reference."]);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference — no matching report source exists", ex.StatusCode);
            return GetResult<SearchResultFloodReportSource>.Failure(["No flood report source found for reference."]);
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

    public async Task<SearchFloodReportPagedResult<SearchResultFloodReport>> SearchFloodReports(DateTimeOffset? resultsFrom, DateTimeOffset? resultsTo, ICollection<Guid> floodSources, long? uprn, long? usrn, string? postcode, MultiPolygon? geom, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
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
        var pagedResults = await downstreamApi.CallApiForUserAsync<object, SearchFloodReportPagedResult<SearchResultFloodReport>>(
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
                    return JsonSerializer.Deserialize<SearchFloodReportPagedResult<SearchResultFloodReport>>(stream, _jsonSerializerOptions);
                };
            },
            cancellationToken: ct);

        if (pagedResults is null)
        {
            logger.LogWarning("No flood reports found");
            return new([], 0, 0, pageSize, pageNumber, 0);
        }

        logger.LogInformation("Successfully retrieved {TotalCount} flood reports", pagedResults.TotalCount);
        return pagedResults;
    }
}