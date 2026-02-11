using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.Options.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
    ];

    private Models.Search.SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        editContext = new(Model);
        editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var options = await GetSearchOptions();
            Model.ToDate = options.ToDate;
            Model.FromDate = options.FromDate;
            Model.SearchType = options.SearchType;
            Model.ByPostcode = options.ByPostcode;
            Model.ByName = options.ByName;
            Model.EastingText = options.EastingText;
            Model.NorthingText = options.NorthingText;
            Model.BufferText = options.BufferText;

            _isLoading = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    private async Task OnSubmit()
    {
        //await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);

        // Go to the next page or pass back to the summary
        var nextPage = GetNextPage();
        var nextPageUrl = nextPage.Url;
        navigationManager.NavigateTo(nextPageUrl);
    }

    private PageInfo GetNextPage()
    {
        // Add filters for specific options
        switch (Model.SearchType)
        {
            case FloodLocationSearchType.All:
                return SearchPages.Results;
            case FloodLocationSearchType.WithFloodReports:
                return SearchPages.Results;
            case FloodLocationSearchType.ByLocation:
                return SearchPages.Location;

            default:
                // Failure, just return the current page
                return SearchPages.Options;
        }
    }

    private async Task<SearchOptionsViewModel> GetSearchOptions()
    {
        var data = await protectedSessionStorage.GetAsync<SearchOptionsViewModel>(SessionConstants.SearchOptions);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Search options were not found in the protected storage.");
        return new SearchOptionsViewModel();
    }
}