using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Details(
    ILogger<Details> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository,
    IFloodEventRepository floodEventRepository,
    IFloodEventState floodEventState,
    ISavedSearchRepository savedSearchRepository,
    IAuthorizationService authorizationService
) : IAsyncDisposable
{
    [Parameter]
    public string? SearchReference { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private ICollection<FloodStatus> _floodStatuses = [];
    private ICollection<EventStatus> _eventStatuses = [];
    private ICollection<InvestigationStatus> _investigationStatuses = [];

    private readonly CancellationTokenSource _cts = new();
    private EditContext? EditContext;
    private EventDetailsViewModel Model = default!;
    private ValidationMessageStore MessageStore = default!;
    private bool _redirectingToSearchOptions;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchReference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            navigationManager.NavigateTo(SearchPages.Options.Url);
            _redirectingToSearchOptions = true;
            return;
        }

        // Don't get any data if you don't have permission
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood event details without authentication");
            return;
        } else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood event details without authentication");
                return;
            }
        }
        
        var writerPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Writer);
        var hasWriterPolicy = writerPolicyCheck.Succeeded;

        if (hasWriterPolicy)
        {
            // Setup model and edit context
            Model = await GetModel();
            EditContext = new(Model);
            EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            MessageStore = new ValidationMessageStore(EditContext);

            // Pre-populate the search title if it's empty
            await PopulateSearchTitle();

            floodEventState.SetEventDetails(Model);

            // Load the options
            _floodStatuses = await commonRepository.GetFloodStatuses(_cts.Token);
            _eventStatuses = await floodEventRepository.GetEventStatuses(_cts.Token);
            _investigationStatuses = await commonRepository.GetInvestigationStatuses(_cts.Token);
        }
    }

    private async Task<EventDetailsViewModel> GetModel()
    {
        // SearchReference is needed for model state and saved search
        if (string.IsNullOrWhiteSpace(SearchReference))
        {
            // default view model with default values
            return new EventDetailsViewModel();
        }

        // First try to get the model from the state
        var modelState = floodEventState.Details;
        if (modelState is not null)
        {
            if (modelState.SearchReference.Equals(SearchReference, StringComparison.Ordinal))
            {
                return modelState;
            }
        }

        // Next try to get the saved search from the database
        var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
        if (savedSearch is not null)
        {
            return new EventDetailsViewModel(savedSearch);
        }

        // When no model state, and no saved search in the database
        return new EventDetailsViewModel
        {
            SearchReference = SearchReference,
        };
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Details));
        }

        GC.SuppressFinalize(this);
    }

    private async Task OnValidSubmit()
    {
        // Save the record
        if (EditContext is null || string.IsNullOrWhiteSpace(SearchReference))
        {
            return;
        }

        var errorFieldIdentifier = EditContext.Field(nameof(Model.SearchReference));

        var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
        if (savedSearch is null)
        {
            MessageStore.Add(errorFieldIdentifier, "The saved search could not be found. Please go back and try again.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var newFloodEvent = Model.ToFloodEvent(savedSearch.Id, Model, floodEventState.Estimates);
        var createdFloodEventResult = await floodEventRepository.Create(newFloodEvent, _cts.Token);
        if (!createdFloodEventResult.IsSuccess)
        {
            foreach (var error in createdFloodEventResult.Errors)
            {
                MessageStore.Add(errorFieldIdentifier, error);
            }
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var floodEvent = createdFloodEventResult.Value;
        savedSearch.SearchTitle = Model.SearchTitle;
        savedSearch.FloodEventId = floodEvent.Id;
        savedSearch.UpdatedBy = await GetUserName();
        var updateSavedSearchResult = await savedSearchRepository.Update(SearchReference, savedSearch, _cts.Token);
        if (!updateSavedSearchResult.IsSuccess)
        {
            foreach (var error in updateSavedSearchResult.Errors)
            {
                MessageStore.Add(errorFieldIdentifier, error);
            }
            EditContext.NotifyValidationStateChanged();
            return;
        }

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");
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

    private async Task PopulateSearchTitle()
    {
        // don't populate the title if it already has one
        if (!string.IsNullOrWhiteSpace(Model.SearchTitle))
        {
            return;
        }

        var (fromDate, toDate, floodSources) = await GetTitleParts();

        // Now build the title using from date, to date and flood sources
        var titleParts = new List<string>();

        // Get year and month from search dates
        DateTimeOffset? floodDate = fromDate ?? toDate;
        if (floodDate.HasValue)
        {
            titleParts.Add(floodDate.Value.Year.ToString(CultureInfo.InvariantCulture));
            titleParts.Add(floodDate.Value.ToString("MMMM", CultureInfo.InvariantCulture));
        }
        else
        {
            titleParts.Add("[Add the date]");
        }

        // Add placeholder text for user to add a location
        titleParts.Add("[Add the location]");

        // Get flood type(s) from flood sources
        if (floodSources.Count > 0)
        {
            var selectedFloodSourceIds = floodSources.ToHashSet();
            var floodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
            var selectedSources = floodProblems
                .Where(fp => selectedFloodSourceIds.Contains(fp.Id))
                .Select(fp => fp.TypeName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (selectedSources.Count > 0)
            {
                titleParts.Add($"({string.Join(", ", selectedSources)})");
            }
        }
        else
        {
            titleParts.Add("(Flood type unknown)");
        }

        if (titleParts.Count > 0)
        {
            Model.SearchTitle = string.Join(' ', titleParts);
        }
    }

    /// <summary>
    ///     <para>Try to get the parts needed for the title from the search results state</para>
    ///     <para>The parts needed are from date, to date and flood sources</para>
    ///     <para>If not available then try to get them from the saved search in the database</para>
    ///     <para>If neither are available return nulls and an empty collection</para>
    /// </summary>
    private async Task<(DateTimeOffset? fromDate, DateTimeOffset? toDate, ICollection<Guid> floodSources)> GetTitleParts()
    {
        if (!string.IsNullOrWhiteSpace(SearchReference))
        {
            // Get the saved search from database
            SavedSearch? savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
            if (savedSearch != null)
            {
                // Parts from the saved search
                return (savedSearch.ResultsFrom, savedSearch.ResultsTo, savedSearch.FloodSources);
            }
        }

        return (null, null, []);
    }

    private async Task GoToEstimates()
    {
        if (EditContext is null || !EditContext.Validate() || string.IsNullOrWhiteSpace(SearchReference))
        {
            return;
        }

        if (floodEventState.HasEstimates && string.Equals(floodEventState.Estimates?.SearchReference, SearchReference, StringComparison.Ordinal))
        {
            var estimates = floodEventState.Estimates;
            estimates?.EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded;
            estimates?.EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly;
            estimates?.EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities;
            floodEventState.SetEventEstimates(estimates);
        }
        else
        {
            var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
            EventEstimatesViewModel estimates;
            if (savedSearch != null)
            {
                estimates = new();

                estimates = estimates.ToModel(savedSearch);
                estimates.EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded;
                estimates.EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly;
                estimates.EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities;
                estimates.SearchReference = SearchReference;
            } else
            {
                estimates = new EventEstimatesViewModel
                {
                    EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded,
                    EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly,
                    EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities,
                    SearchReference = SearchReference,
                };
            }

            floodEventState.SetEventEstimates(estimates);
        }

        navigationManager.NavigateTo($"{EventPages.Estimates.Url}/{SearchReference}");
    }
}