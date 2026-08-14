using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager
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
    private bool _isReferenceKnown;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        editContext = new(Model);
        editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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
        // Go to the next page or pass back to the summary
        var nextPage = GetNextPage();
        var nextPageUrl = nextPage.Url;
        if (_isReferenceKnown)
        {
            nextPageUrl += "/" + Model.ByReference;
        }
        navigationManager.NavigateTo(nextPageUrl);
    }

    private PageInfo GetNextPage()
    {
        // Add filters for specific options
        if (_isReferenceKnown)
        {
            return SearchPages.Results;
        }
        
        return SearchPages.Options; 
    }
}