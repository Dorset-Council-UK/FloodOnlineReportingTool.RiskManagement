using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.Home.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
    ];

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _isReferenceKnown;

    private IReadOnlyCollection<GdsOptionItem<bool>> _isReferenceKnownOptions = [
        new("ref-known-yes", "Yes", value: true),
        new("ref-known-no", "No", value: false),
    ];

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
            Model.ByReference = options.ByReference;
            _isReferenceKnown = !string.IsNullOrWhiteSpace(Model.ByReference);

            _isLoading = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
        }
        catch (ObjectDisposedException)
        {
            // Cancellation token source already disposed; nothing to do.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while disposing {ComponentName}.", nameof(Index));
        }

        _cts.Dispose();
    }

    private async Task OnSubmit()
    {
        //Store the search options in session storage for retrieval on the next page
        await protectedSessionStorage.SetAsync(SessionConstants.SearchOptions, Model);

        // Go to the next page or pass back to the summary
        var nextPage = GetNextPage();
        var nextPageUrl = nextPage.Url;
        navigationManager.NavigateTo(nextPageUrl);
    }

    private PageInfo GetNextPage()
    {
        // Add filters for specific options
        if (_isReferenceKnown)
        {
            return SearchPages.Results;
        } else
        {
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