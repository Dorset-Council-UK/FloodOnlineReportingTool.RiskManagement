using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();

    [Parameter]
    public string? SearchReference { get; set; }
    private string _backURL { get; set; } = "";

    private bool _isLoading = true;

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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Results));
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

                _isLoading = false;
                StateHasChanged();
                return;
            }
            _backURL = $"{SearchPages.Results.Url}/{SearchReference}";

            _isLoading = false;
            StateHasChanged();
        }
    }
}