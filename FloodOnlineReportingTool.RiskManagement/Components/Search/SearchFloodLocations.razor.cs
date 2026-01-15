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
    NavigationManager navigationManager,
    IWebHostEnvironment environment
) {
    private const int PageSize = 5;

    // Fields
    private EditContext? _editContext;
    private ValidationMessageStore? _messageStore;

    // Fields - Search state
    private SearchState? _searchState;
    private PagedResult<SearchResultFloodLocation>? _pagedSearch;
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
        _messageStore ??= new(_editContext);

        // Search state
        _searchState ??= SearchState.NotStarted;
        _pagedSearch ??= new([], 0, PageSize, 1, 0);
        _searchTypeOptions ??= [
            new ("searchAll", "All", FloodLocationSearchType.All),
            new ("searchWithReports", "Include flood reports", FloodLocationSearchType.WithFloodReports),
            new ("searchByLocation", "Near location", FloodLocationSearchType.ByLocation),
        ];
    }

    protected override void OnParametersSet()
    {
        var searchType = Enum.TryParse<FloodLocationSearchType>(SearchTypeQuery, ignoreCase: true, out var parsedType)
            ? parsedType
            : FloodLocationSearchType.All;

        // Update the model from the query parameters
        Model!.PageNumber = PageNumberQuery is > 1 ? PageNumberQuery : 1;
        if (searchType == FloodLocationSearchType.ByLocation)
        {
            Model.EastingText = EastingQuery?.ToString(CultureInfo.InvariantCulture);
            //Model.Easting = EastingQuery;
            Model.NorthingText = NorthingQuery?.ToString(CultureInfo.InvariantCulture);
            //Model.Northing = NorthingQuery;
            Model.Buffer = BufferQuery;
        }
        else
        {
            Model.EastingText = null;
            //Model.Easting = null;
            Model.NorthingText = null;
            //Model.Northing = null;
            Model.Buffer = null;
        }
        Model.SearchType = searchType;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await AutoSearch();
            StateHasChanged();
        }
    }

    private async Task OnSearchSubmit()
    {
        // Reset to first page on new search
        Model!.PageNumber = 1;
        if (!ValidSearch())
        {
            return;
        }
        UpdateUrlAndNavigate();
    }

    private bool ValidSearch()
    {
        _messageStore!.Clear();
        return _editContext!.Validate();
    }

    /// <summary>
    /// Updates the browser's URL to reflect the current search parameters and navigates to the updated URL.
    /// </summary>
    private void UpdateUrlAndNavigate()
    {
        var isByLocation = Model!.SearchType == FloodLocationSearchType.ByLocation;
        var parameters = new Dictionary<string, object?>(StringComparer.CurrentCulture)
        {
            { "searchType", Model.SearchType == FloodLocationSearchType.All ? null : Model.SearchType.ToString() },
            { "page", Model.PageNumber is > 1 ? Model.PageNumber : null },
            { "easting", isByLocation ? Model.Easting : null },
            { "northing", isByLocation ? Model.Northing : null },
            { "buffer", isByLocation ? Model.Buffer : null },
        };

        var uri = navigationManager.GetUriWithQueryParameters(parameters);
        var forceLoad = true; // Allow searching again with the same parameters
        navigationManager.NavigateTo(uri, forceLoad, replace: false);
    }

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
    private async Task AutoSearch()
    {
        if (!ValidSearch())
        {
            return;
        }

        _searchState = SearchState.Searching;

        try
        {
            var pageNumber = Model!.PageNumber ?? 1;

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

            var formErrorField = FieldIdentifier.Create(() => Model!.PageNumber);
            _messageStore!.Add(formErrorField,
            [
                "There was a problem searching for flood locations",
                "Please try again and if the problem persists, contact support.",
            ]);
            if (environment.IsDevelopment())
            {
                _messageStore.Add(formErrorField, ex.Message);
            }
            _editContext!.NotifyValidationStateChanged();
        }
    }

    private string ResultsFoundText()
    {
        const string template1 = "{0} result found";
        const string templateN = "{0} results found";

        var found = _searchState == SearchState.ResultsFound ? _pagedSearch!.TotalCount : 0;
        var template = found == 1 ? template1 : templateN;

        return string.Format(CultureInfo.InvariantCulture, template, found);
    }

    private async Task OnPageChanged(int newPageNumber)
    {
        Model!.PageNumber = newPageNumber is > 1 ? newPageNumber : 1;
        if (!ValidSearch())
        {
            return;
        }
        UpdateUrlAndNavigate();
    }
}
