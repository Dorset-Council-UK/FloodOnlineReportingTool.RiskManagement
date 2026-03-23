using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(ILogger<Results> logger, ICommonRepository commonRepository, ISearchResultsState searchResultsState, NavigationManager navigationManager) : IAsyncDisposable
{
    private EditContext searchContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel Model { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "ref")]
    public required string SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation; no action required.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Options));
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (string.IsNullOrEmpty(SearchReference))
            {
                logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
                navigationManager.NavigateTo(SearchPages.Options.Url);
            }

            var options = searchResultsState.Options;
            if (options is null || !string.Equals(options.ByReference, SearchReference, StringComparison.OrdinalIgnoreCase))
            {
                var savedSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);

                if (savedSearch == null)
                {
                    // Failure - No saved search found for reference
                    _isLoading = false;
                    StateHasChanged();
                    return;
                }

                // Convert Entity → ViewModel
                Model = savedSearch.SavedSearchToResultsView();
                
                // Persist in state for page navigation
                searchResultsState.SetResultOptions(Model);
            }
            else
            {
                Model = options;
            }

            _isLoading = false;
            StateHasChanged();
        }
    }
}