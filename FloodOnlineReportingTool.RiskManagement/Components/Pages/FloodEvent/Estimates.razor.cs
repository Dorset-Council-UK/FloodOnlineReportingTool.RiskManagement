using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Estimates(
    ILogger<Estimates> logger,
    NavigationManager navigationManager,
    IFloodEventState floodEventState,
    ICommonRepository commonRepository,
    IFloodEventRepository floodEventRepository) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private EventEstimatesViewModel Model { get; set; } = default!;
    private EventEstimatesViewModel? _modelFromState = null;
    private EditContext EditContext = default!;

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
        EditContext = new(Model);
        EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_modelFromState == null)
        {
            _modelFromState = floodEventState.Estimates;
            if (_modelFromState != null && string.Equals(_modelFromState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
            {
                Model = _modelFromState;
                EditContext = new(Model);
                EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            }
            else
            {
                var databaseModel = await commonRepository.GetSavedSearch(SearchReference, _cts.Token);
                if (databaseModel != null)
                {
                    Model = Model.ToModel(databaseModel);
                    EditContext = new(Model);
                    EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                }

                // Clear the existing state if the search reference does not match, to avoid displaying stale data
                Model.SearchReference = SearchReference ?? "";
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Estimates));
        }
    }

    private async Task OnValidSubmit()
    {
        // Save the record
        if (SearchReference == null)
        {
            return;
        }

        var currentSearch = await commonRepository.GetSavedSearch(SearchReference, _cts.Token);
        if (currentSearch == null)
        {
            return;
        }

        var eventDetails = floodEventState.Details;
        if (eventDetails == null) 
        {
            return;
        }
        var floodEvent = Model.ToFloodEvent(eventDetails, Model);
        await floodEventRepository.CreateOrUpdateFloodEvent(currentSearch.Id, floodEvent, _cts.Token);

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
        if (EditContext.Validate())
        {
            navigationManager.NavigateTo($"{EventPages.Details.Url}/{SearchReference}");
        }
        StateHasChanged();
        return;
    }
}