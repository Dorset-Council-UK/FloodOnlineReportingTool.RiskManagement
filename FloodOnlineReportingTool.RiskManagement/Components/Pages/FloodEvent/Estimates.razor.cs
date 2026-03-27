using FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using MassTransit.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Estimates(
    ILogger<Estimates> logger,
    NavigationManager navigationManager,
    IFloodEventState floodEventState,
    ICommonRepository commonRepository) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private EventEstimatesViewModel Model { get; set; } = default!;
    private EventEstimatesViewModel? _modelFromState = null;
    private EditContext estimatesContext = default!;

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(SearchReference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            navigationManager.NavigateTo(SearchPages.Options.Url);

            _isLoading = false;
            StateHasChanged();
            return;
        }

        // Setup model and edit context
        Model ??= new();
        estimatesContext = new(Model);
        estimatesContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_modelFromState == null)
        {
            _modelFromState = floodEventState.Estimates;
            if (_modelFromState != null && string.Equals(_modelFromState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
            {
                Model = _modelFromState;
                estimatesContext = new(Model);
                estimatesContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            }
            else
            {
                // TODO: Check if the options need picking up from the database? 

                // Clear the existing state if the search reference does not match, to avoid displaying stale data
                floodEventState.SetEventEstimates(Model);
            }
        }

        _isLoading = false;
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

    private async Task OnValidSubmit()
    {
        // Save the record

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }

    private void BackToDetails()
    {
        Model.SearchReference = SearchReference ?? "";
        navigationManager.NavigateTo($"{EventPages.Details.Url}/{SearchReference}");
        StateHasChanged();
        return;
    }
}