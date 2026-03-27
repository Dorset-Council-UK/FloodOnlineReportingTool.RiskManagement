using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Helpers;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;
public partial class Details(
    ILogger<Details> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository, 
    IFloodEventState floodEventState,
    ISearchResultsState searchResultsState) : IAsyncDisposable
{
    private EditContext eventContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private EventDetailsViewModel Model { get; set; } = default!;
    private EventDetailsViewModel? _modelFromState = null;
    private SearchResultsViewModel? SearchResults;

    [Parameter]
    public string? SearchReference { get; set; }

    private Dictionary<Guid, string> _floodStatuses = new();
    private Dictionary<Guid, string> _eventStatuses = new();
    private Dictionary<Guid, string> _investigationStatuses = new();

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        eventContext = new(Model);
        eventContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnParametersSetAsync()
    {
        if (searchResultsState.HasOptions)
        {
            SearchResults = searchResultsState.Options;
        }

        // Configure option lists once on page load - No selections these are fixed option lists
        _floodStatuses = _floodStatuses.Count == 0 
            ? (await commonRepository.GetFloodStatuses(_cts.Token)).ToDictionary(x => x.Id, x => x.Text ?? string.Empty) 
            : _floodStatuses;
        _eventStatuses = _eventStatuses.Count == 0 
            ? (await commonRepository.GetEventStatuses(_cts.Token)).ToDictionary(x => x.Id, x => x.Text ?? string.Empty) 
            : _eventStatuses;
        _investigationStatuses = _investigationStatuses.Count == 0 
            ? (await commonRepository.GetInvestigationStatuses(_cts.Token)).ToDictionary(x => x.Id, x => x.Text ?? string.Empty) 
            : _investigationStatuses;

        if (_modelFromState == null)
        {
            _modelFromState = floodEventState.Details;
            if (_modelFromState != null && string.Equals(_modelFromState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
            {
                Model = _modelFromState;
                eventContext = new(Model);
                eventContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            }
            else
            {
                // TODO: Check if the options need picking up from the database? 

                // Clear the existing state if the search reference does not match, to avoid displaying stale data
                floodEventState.SetEventDetails(Model);
            }
        }
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
        // Save the record

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }

    private void GoToEstimates()
    {
        Model.SearchReference = SearchReference ?? "";
        var existingDetails = floodEventState.Details;
        if (floodEventState.HasEstimates)
        {
            var estimates = floodEventState.Estimates;
            estimates.EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded;
            estimates.EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly;
            estimates.EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities;
            floodEventState.SetEventEstimates(estimates);
        } else
        {
            EventEstimatesViewModel estimates = new EventEstimatesViewModel
            {
                EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded,
                EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly,
                EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities,
                SearchReference = SearchReference
            };
            floodEventState.SetEventEstimates(estimates);
        }
         
        navigationManager.NavigateTo($"{EventPages.Estimates.Url}/{SearchReference}");

        StateHasChanged();
        return;
    }
}