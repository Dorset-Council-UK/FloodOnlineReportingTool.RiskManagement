using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodLocations(
    ILogger<SearchFloodLocations> logger,
    IGdsJsInterop gdsJs,
    IReportStatusClient reportStatusClient,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler
) {
    private const int PageSize = 20;

    private SearchFloodLocationsViewModel Model { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;

    // Search
    private SearchState _searchState = SearchState.NotStarted;
    private PagedResult<SearchResultFloodLocation> _pagedSearch = default!;
    private IReadOnlyCollection<string> _searchErrors = [];
    private readonly IReadOnlyCollection<GdsOptionItem<FloodLocationSearchType>> _searchTypeOptions = [
        new ("searchAll", "All", FloodLocationSearchType.All),
        new ("searchWithReports", "Include flood reports", FloodLocationSearchType.WithFloodReports),
        new ("searchByLocation", "Near location", FloodLocationSearchType.ByLocation),
    ];

    protected override void OnInitialized()
    {
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _pagedSearch = new([], 0, Model.PageSize ?? 10, Model.PageNumber ?? 1, 0);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnSearch()
    {
        _searchErrors = [];
        var validated = _editContext.Validate();
        if (!validated)
        {
            return;
        }

        await OnSearchFloodLocations();
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
            consentHandler.HandleException(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while searching flood locations");

            _searchState = SearchState.Error;

            _searchErrors = [
                "There was a problem sarching for flood locations",
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
