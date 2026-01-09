using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodLocations(
    ILogger<SearchFloodLocations> logger,
    IReportStatusClient reportStatusClient,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
    NavigationManager navigationManager
) {
    // Constants
    private const int PageSize = 20;

    // Fields
    private EditContext? _editContext;

    // Fields - Search state
    private SearchState? _searchState;
    private PagedResult<SearchResultFloodLocation>? _pagedSearch;
    private ICollection<string>? _searchErrors;
    private IReadOnlyCollection<GdsOptionItem<FloodLocationSearchType>>? _searchTypeOptions;

    // Properties
    private SearchFloodLocationsViewModel? Model { get; set; }

    // Properties - Search query
    [SupplyParameterFromQuery(Name = "searchType")]
    private string? SearchTypeQuery { get; set; }
    [SupplyParameterFromQuery(Name = "page")]
    private int? PageNumberQuery { get; set; }
    [SupplyParameterFromQuery(Name = "easting")]
    private double? EastingQuery { get; set; }
    [SupplyParameterFromQuery(Name = "northing")]
    private double? NorthingQuery { get; set; }
    [SupplyParameterFromQuery(Name = "buffer")]
    private double? BufferQuery { get; set; }

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        if (_editContext is null)
        {
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        // Search state
        _searchState ??= SearchState.NotStarted;
        _pagedSearch ??= new([], 0, PageSize, 1, 0);
        _searchErrors ??= [];
        _searchTypeOptions ??= [
            new ("searchAll", "All", FloodLocationSearchType.All),
            new ("searchWithReports", "Include flood reports", FloodLocationSearchType.WithFloodReports),
            new ("searchByLocation", "Near location", FloodLocationSearchType.ByLocation),
        ];
    }

    protected override async Task OnParametersSetAsync()
    {
        CopyQueryParametersToModel();

        // Auto-search if we have URL parameters and haven't searched yet
        if (_searchState == SearchState.NotStarted && HasSearchQueryParameters())
        {
            await OnSearchFloodLocations();
        }
    }

    private void CopyQueryParametersToModel()
    {
        if (Enum.TryParse<FloodLocationSearchType>(SearchTypeQuery, ignoreCase: true, out var searchType))
        {
            Model.SearchType = searchType;
        }

        Model.PageNumber = PageNumberQuery;

        if (searchType == FloodLocationSearchType.ByLocation)
        {
            Model.Easting = EastingQuery;
            Model.EastingText = EastingQuery?.ToString(CultureInfo.InvariantCulture);
            Model.Northing = NorthingQuery;
            Model.NorthingText = NorthingQuery?.ToString(CultureInfo.InvariantCulture);
            Model.Buffer = BufferQuery;
        }
    }

    private bool HasSearchQueryParameters()
    {
        return SearchTypeQuery is not null || PageNumberQuery.HasValue || EastingQuery.HasValue || NorthingQuery.HasValue;
    }

    private async Task OnSearch()
    {
        _searchErrors = [];
        var validated = _editContext.Validate();
        if (!validated)
        {
            return;
        }

        // Update URL with current model state
        UpdateUrlWithModelState();

        await OnSearchFloodLocations();
    }

    private void UpdateUrlWithModelState()
    {
        var isByLocation = Model.SearchType == FloodLocationSearchType.ByLocation;
        var parameters = new Dictionary<string, object?>(StringComparer.CurrentCulture)
        {
            { "searchType", Model.SearchType.ToString() },
            { "page", Model.PageNumber },
            { "easting", isByLocation ? Model.Easting : null },
            { "northing", isByLocation ? Model.Northing : null },
            { "buffer", isByLocation ? Model.Buffer : null },
        };

        var uri = navigationManager.GetUriWithQueryParameters(parameters);
        navigationManager.NavigateTo(uri, replace: false);
    }

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
    private async Task OnSearchFloodLocations()
    {
        _searchState = SearchState.Searching;

        try
        {
            var pageNumber = Model.PageNumber ?? 1;

            _pagedSearch = Model.SearchType switch
            {
                FloodLocationSearchType.All =>
                    await reportStatusClient.SearchFloodLocations(pageNumber, PageSize),
                FloodLocationSearchType.WithFloodReports =>
                    await reportStatusClient.SearchFloodLocationsWithFloodReports(pageNumber, PageSize),
                FloodLocationSearchType.ByLocation when Model.Easting is double easting && Model.Northing is double northing && Model.Buffer is double buffer =>
                    await reportStatusClient.SearchFloodLocationsByPoint(easting, northing, buffer, pageNumber, PageSize),
                _ => new([], 0, PageSize, pageNumber, 0),
            };

            if (_pagedSearch is null || _pagedSearch.TotalCount == 0)
            {
                _searchState = SearchState.NoResults;
            }

            _searchState = SearchState.ResultsFound;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while searching flood locations");
            consentHandler.HandleException(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while searching flood locations");

            _searchState = SearchState.Error;

            _searchErrors = [
                "There was a problem searching for flood locations",
                "Please try again and if the problem persists, contact support.",
            ];
        }
    }

    private string ResultsFoundText()
    {
        const string template1 = "{0} result found";
        const string templateN = "{0} results found";

        var found = _searchState == SearchState.ResultsFound ? _pagedSearch.TotalCount : 0;
        var template = found == 1 ? template1 : templateN;

        return string.Format(CultureInfo.InvariantCulture, template, found);
    }

    private async Task OnPageChanged(int newPageNumber)
    {
        Model.PageNumber = newPageNumber;
        await OnSearch();
    }
}
