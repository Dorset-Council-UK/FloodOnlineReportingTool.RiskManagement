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
    private bool _isSearching = false;
    private SearchResult? _searchResult;

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

    private async Task OnValidSubmit()
    {
        if (Search.Uprn is null)
        {
            return;
        }

        _isSearching = true;
        long uprn = Search.Uprn.Value;
        logger.LogInformation("Searching food reports with UPRN {Uprn}", uprn);
        _searchResult = await reportStatusApiService.SearchUprn(uprn).ConfigureAwait(false);
        _isSearching = false;
    }
}
