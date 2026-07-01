using FloodOnlineReportingTool.Contracts.Shared.Models;
using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Services;

/// <summary>
/// Report Status API client using Downstream API
/// </summary>
public interface IReportStatusClient
{
    /// <summary>
    /// Searches for flood locations using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood locations and related flood reports using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood locations near a point using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodLocation>> SearchFloodLocationsByPoint(double easting, double northing, double buffer, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood properties using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodProperty>> SearchFloodProperties(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood properties and related flood reports using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodProperty>> SearchFloodPropertiesWithFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood reports using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood reports within metres using the downstream API with pagination
    /// </summary>
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsWithinMetres(double easting, double northing, double metres = 50, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a flood report exists for the given reference in the downstream API.
    /// Returns a <see cref="SearchRedirect"/> with the report Id if found, used for redirect
    /// routing only — not for display purposes.
    /// </summary>
    Task<SearchRedirect?> FindFloodReportByReference(string reference, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the full flood report detail for display on the details page.
    /// Internally maps <see cref="FloodReportWithSourceReferences"/> to <see cref="SearchResultFloodReport"/>.
    /// </summary>
    Task<GetResult<SearchResultFloodReport>> GetFloodReportDetail(string reference, bool firstSourceOnly, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood reports by UPRN using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUprn(long uprn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood reports by USRN using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodReport>> SearchFloodReportsByUsrn(long usrn, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood roads using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoads(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood roads and related flood reports using the downstream API with pagination
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<PagedResult<SearchResultFloodRoad>> SearchFloodRoadsFloodReports(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches for flood reports using the downstream API with pagination and filtering
    /// </summary>
    /// <remarks>
    /// Exceptions must be handled by Blazor to allow the use of <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// </remarks>
    Task<SearchFloodReportPagedResult<SearchResultFloodReport>> SearchFloodReports(DateTimeOffset? resultsFrom, DateTimeOffset? resultsTo, ICollection<Guid> floodSources, long? uprn, long? usrn, string? postcode, MultiPolygon? geom, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
}
