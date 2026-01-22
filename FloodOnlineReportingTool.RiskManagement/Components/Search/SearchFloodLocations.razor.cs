using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Identity.Web;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodLocations(
    ILogger<SearchFloodLocations> logger,
    IReportStatusClient reportStatusClient,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
    IWebHostEnvironment environment,
    ProtectedSessionStorage protectedSessionStore
) {
    private const int PageSize = 5;

    private EditContext _editContext = null!;
    private ValidationMessageStore _messageStore = null!;
    private SearchState? _searchState;
    private PagedResult<SearchResultFloodLocation> _pagedSearch = null!;
    private IReadOnlyCollection<GdsOptionItem<FloodLocationSearchType>> _searchTypeOptions = [];

    public SearchFloodLocationsViewModel Model { get; set; } = null!;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var storedModel = await GetStoredModel();
        Model = storedModel ?? new()
        {
            PageNumber = 1,
            Buffer = 10,
            SearchType = FloodLocationSearchType.All,
        };

        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new(_editContext);
        _searchState = SearchState.NotStarted;
        _pagedSearch = new([], 0, PageSize, 1, 0);
        _searchTypeOptions = [
            new ("searchAll", "All", FloodLocationSearchType.All),
            new ("searchWithReports", "Include flood reports", FloodLocationSearchType.WithFloodReports),
            new ("searchByLocation", "Near location", FloodLocationSearchType.ByLocation),
        ];
    }

    private async Task<SearchFloodLocationsViewModel?> GetStoredModel()
    {
        var result = await protectedSessionStore.GetAsync<SearchFloodLocationsViewModel>("search-locations");
        if (result.Success && result.Value is not null)
        {
            return result.Value;
        }

        return null;
    }

    private async Task OnSearchSubmit()
    {
        // Reset to first page on new search
        Model.PageNumber = 1;
        if (ValidSearch())
        {
            await protectedSessionStore.SetAsync("search-locations", Model);
            await Search();
        }
    }

    private bool ValidSearch()
    {
        _messageStore.Clear();
        return _editContext.Validate();
    }

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
    private async Task Search()
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

            var formErrorField = FieldIdentifier.Create(() => Model.PageNumber);
            _messageStore.Add(formErrorField,
            [
                "There was a problem searching for flood locations",
                "Please try again and if the problem persists, contact support.",
            ]);
            if (environment.IsDevelopment())
            {
                _messageStore.Add(formErrorField, ex.Message);
            }
            _editContext.NotifyValidationStateChanged();
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
        Model.PageNumber = newPageNumber is > 1 ? newPageNumber : 1;
        if (ValidSearch())
        {
            await protectedSessionStore.SetAsync("search-locations", Model);
            await Search();
        }
    }
}
