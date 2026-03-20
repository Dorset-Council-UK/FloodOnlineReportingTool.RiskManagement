using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using MassTransit.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Estimates(
    ILogger<Estimates> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private EventEstimatesViewModel Model { get; set; } = default!;
    private EditContext estimatesContext = default!;

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        estimatesContext = new(Model);
        estimatesContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {

        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }
}