using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Settings;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodLocations(
    ILogger<SearchFloodLocations> logger,
    IGdsJsInterop gdsJs,
    IReportStatusClient reportStatusClient,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler
) {
    private const string TryAgain = "Please try again and if the problem persists, contact support.";

    private SearchFloodLocationsViewModel Model { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;
    //private PagedResult<SearchResultFloodLocation> _search;
    private SearchResult<PagedResult<SearchResultFloodLocation>>? _pagedSearch;
    private SearchState _searchState = SearchState.NotStarted;

    protected override void OnInitialized()
    {
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
    private async Task OnSearchFloodLocations()
    {
        var validated = _editContext.Validate();
        if (!validated)
        {
            return;
        }

        _searchState = SearchState.Searching;

        /*
        if (Model.Uprn is not null)
        {
            long uprn = Model.Uprn.Value;
            logger.LogInformation("Searching flood reports with UPRN {Uprn}", uprn);

            try
            {
                _searchResult = await SearchFloodReportsByUprn(uprn);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                consentHandler.HandleException(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search flood reports by UPRN {Uprn} failed.", uprn);
                _searchResult = SearchResult.Failure([
                    "I am sorry but there was a problem searching using that UPRN.",
                    ex.Message,
                    TryAgain,
                ]);
            }
        }
        */

        _searchState = CurrentSearchState();

        return;
    }

    private SearchState CurrentSearchState()
    {
        if (_search is null)
        {
            return SearchState.NotStarted;
        }

        if (_search.e)
        if (_searchResult.Errors.Count > 0)
        {
            return SearchState.Error;
        }

        return _searchResult.FloodReports.Count > 0
            ? SearchState.ResultsFound
            : SearchState.NoResults;
    }

    /*
    private async Task<SearchResult> SearchFloodReportsByUprn(long uprn)
    {
        var floodReports = await downstreamApi
            .CallApiForUserAsync<IReadOnlyCollection<TEMP_FloodReport>>(RiskManagementSettings.ReportStatusApi, options =>
            {
                options.RelativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/uprn?uprn={uprn}");
            });

        return SearchResult.Success(floodReports ?? []);
    }
    */

    private string ResultsFoundText()
    {
        const string template1 = "{0} result found";
        const string templateN = "{0} results found";

        var numberFound = _searchResult?.Succeeded == true ? _searchResult.Items.
        var found = _searchResult?.FloodReports.Count ?? 0;
        var template = found == 1 ? template1 : templateN;

        return string.Format(CultureInfo.InvariantCulture, template, found);
    }

    private async Task OnPageChanged(int newPageNumber)
    {
        int pageSize = Model.PageSize ?? 10;
        _pagedSearch = await reportStatusClient.SearchFloodLocations(newPageNumber, pageSize);
    }
}
