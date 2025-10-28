using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodReports(ILogger<SearchFloodReports> logger, IGdsJsInterop gdsJs, IReportStatusApiService reportStatusApiService)
{
    private SearchFloodReportViewModel Search { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;
    private SearchResult? _searchResult;
    private SearchState _searchState = SearchState.NotStarted;

    protected override void OnInitialized()
    {
        Search ??= new();
        _editContext = new(Search);
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

    private async Task OnSearchFloodReport()
    {
        var validated = _editContext.Validate();
        if (!validated)
        {
            return;
        }

        _searchState = SearchState.Searching;

        if (Search.Uprn is not null)
        {
            long uprn = Search.Uprn.Value;
            await SearchByUprn(uprn);
        }

        _searchState = CurrentSearchState();

        return;
    }

    private async Task SearchByUprn(long uprn)
    {
        logger.LogInformation("Searching food reports with UPRN {Uprn}", uprn);
        _searchResult = await reportStatusApiService.SearchUprn(uprn).ConfigureAwait(false);
    }

    private SearchState CurrentSearchState()
    {
        if (_searchResult is null)
        {
            return SearchState.NotStarted;
        }

        if (_searchResult.Errors.Count > 0)
        {
            return SearchState.Error;
        }

        return _searchResult.FloodReports.Count > 0
            ? SearchState.ResultsFound
            : SearchState.NoResults;
    }
}
