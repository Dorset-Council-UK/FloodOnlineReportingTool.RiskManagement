using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Estimates(
    ILogger<Estimates> logger,
    NavigationManager navigationManager,
    IFloodEventState floodEventState,
    IFloodEventRepository floodEventRepository,
    ISavedSearchRepository savedSearchRepository
) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private ValidationMessageStore _messageStore = default!;
    private EventEstimatesViewModel Model { get; set; } = default!;
    private EditContext EditContext = default!;
    private bool _estimatesChanged;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

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
        _messageStore = new(EditContext);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchReference))
        {
            _isLoading = false;
            return;
        }

        var estimateState = floodEventState.Estimates;
        var estimatesMatch = estimateState is not null
            && string.Equals(estimateState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase);

        var detailsState = floodEventState.Details;
        var detailsMatch = detailsState is not null
            && string.Equals(detailsState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase);

        SavedSearch? savedSearch = null;
        if (!estimatesMatch || !detailsMatch)
        {
            savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
        }

        if (estimatesMatch)
        {
            Model = estimateState;
        }
        else
        {
            floodEventState.SetEventEstimates(null);

            Model = savedSearch is not null
                ? new EventEstimatesViewModel().ToModel(savedSearch)
                : new EventEstimatesViewModel { SearchReference = SearchReference };

            if (savedSearch?.FloodEvent is not null && HasEstimateValues(Model))
            {
                floodEventState.SetEventEstimates(Model);
            }
        }

        if (!detailsMatch)
        {
            floodEventState.SetEventDetails(savedSearch is not null
                ? new EventDetailsViewModel(savedSearch)
                : new EventDetailsViewModel { SearchReference = SearchReference });
        }

        CreateEditContext();
        _isLoading = false;
    }

    private void CreateEditContext()
    {
        _estimatesChanged = false;
        Model.SearchReference = SearchReference ?? "";
        EditContext = new(Model);
        EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        EditContext.OnFieldChanged += (_, _) => _estimatesChanged = true;
        _messageStore = new(EditContext);
    }

    private static bool HasEstimateValues(EventEstimatesViewModel estimates)
    {
        return estimates.EstimatedResidentialAffected != 0
            || estimates.EstimatedResidentialFlooded != 0
            || estimates.EstimatedResidentialOther != 0
            || estimates.EstimatedCommercialAffected != 0
            || estimates.EstimatedCommercialFlooded != 0
            || estimates.EstimatedCommercialOther != 0
            || estimates.EstimatedNoElectric != 0
            || estimates.EstimatedNoGas != 0
            || estimates.EstimatedNoFoul != 0
            || estimates.EstimatedNoWaterSupply != 0
            || estimates.FloodExtentConfidence is not null
                && estimates.FloodExtentConfidence != ConfidenceOptions.Unknown;
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
        floodEventState.SetEventEstimates(Model);
        var errorFieldIdentifier = EditContext.Field(nameof(Model.SearchReference));

        // Save the record
        if (SearchReference == null)
        {
            _messageStore.Add(errorFieldIdentifier, "Search reference is missing. Unable to save estimates.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
        if (savedSearch == null)
        {
            _messageStore.Add(errorFieldIdentifier, "Saved search not found for the provided search reference. Unable to save estimates.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var eventDetails = floodEventState.Details;
        if (eventDetails == null) 
        {
            _messageStore.Add(errorFieldIdentifier, "Event details are missing. Unable to save estimates.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var newFloodEvent = Model.ToFloodEvent(savedSearch.Id, eventDetails, Model);

        if (savedSearch.FloodEventId is Guid floodEventId)
        {
            var updatedFloodEvent = newFloodEvent with { Id = floodEventId };
            var updateResult = await floodEventRepository.Update(floodEventId, updatedFloodEvent, _cts.Token);
            if (!updateResult.IsSuccess)
            {
                foreach (var error in updateResult.Errors)
                {
                    _messageStore.Add(errorFieldIdentifier, error);
                }
                EditContext.NotifyValidationStateChanged();
                return;
            }

            Model.FloodEventId = floodEventId;
        }
        else
        {
            var createResult = await floodEventRepository.Create(newFloodEvent, _cts.Token);
            if (!createResult.IsSuccess)
            {
                foreach (var error in createResult.Errors)
                {
                    _messageStore.Add(errorFieldIdentifier, error);
                }
                EditContext.NotifyValidationStateChanged();
                return;
            }

            if (createResult.Value is not null)
            {
                savedSearch.FloodEventId = createResult.Value.Id;
                Model.FloodEventId = createResult.Value.Id;
            }
        }

        savedSearch.SearchTitle = Model.SearchTitle;
        if (savedSearch.FloodEventId is null)
        {
            _messageStore.Add(errorFieldIdentifier, "The flood event could not be saved.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        savedSearch.UpdatedBy = await GetUserName();
        var updateSavedSearchResult = await savedSearchRepository.Update(SearchReference, savedSearch, _cts.Token);
        if (!updateSavedSearchResult.IsSuccess)
        {
            foreach (var error in updateSavedSearchResult.Errors)
            {
                _messageStore.Add(errorFieldIdentifier, error);
            }
            EditContext.NotifyValidationStateChanged();
            return;
        }

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");
    }

    private void BackToDetails()
    {
        Model.SearchReference = SearchReference ?? "";
        var estimateState = floodEventState.Estimates;
        var estimatesMatch = estimateState is not null
            && string.Equals(estimateState.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase);

        if (estimatesMatch || _estimatesChanged || HasEstimateValues(Model))
        {
            floodEventState.SetEventEstimates(Model);
        }
        else
        {
            floodEventState.SetEventEstimates(null);
        }

        if (EditContext.Validate())
        {
            navigationManager.NavigateTo($"{EventPages.Details.Url}/{SearchReference}");
        }
        StateHasChanged();
    }

    private async Task<string?> GetUserName()
    {
        if (AuthenticationState is null)
        {
            return null;
        }

        var authState = await AuthenticationState;
        return authState.User.DisplayName;
    }
}