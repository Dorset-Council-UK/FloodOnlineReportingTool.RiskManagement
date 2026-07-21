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
    private EventEstimatesViewModel? _modelFromState = null;
    private EditContext EditContext = default!;

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
                var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
                if (savedSearch != null)
                {
                    Model = Model.ToModel(savedSearch);
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

        var floodEvent = Model.ToFloodEvent(savedSearch.Id, eventDetails, Model);
        var createFloodEventResult = await floodEventRepository.Create(floodEvent, _cts.Token);
        if (!createFloodEventResult.IsSuccess)
        {
            foreach (var error in createFloodEventResult.Errors)
            {
                _messageStore.Add(errorFieldIdentifier, error);
            }
            EditContext.NotifyValidationStateChanged();
            return;
        }

        savedSearch.SearchTitle = Model.SearchTitle;
        savedSearch.FloodEventId = floodEvent.Id;
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
        if (EditContext.Validate())
        {
            navigationManager.NavigateTo($"{EventPages.Details.Url}/{SearchReference}");
        }
        StateHasChanged();
        return;
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