using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
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
    IFloodEventState floodEventState,
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
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                var writerPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Writer);
                var hasWriterPolicy = writerPolicyCheck.Succeeded;

                if (hasWriterPolicy)
                {
                    // Setup model and edit context
                    Model = await GetModel();
                    EditContext = new(Model);
                    EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

                    // Pre-populate the search title if it's empty
                    await PopulateSearchTitle();

                    floodEventState.SetEventDetails(Model);

                    // Load the options
                    _floodStatuses = await commonRepository.GetFloodStatuses(_cts.Token);
                    _eventStatuses = await commonRepository.GetEventStatuses(_cts.Token);
                    _investigationStatuses = await commonRepository.GetInvestigationStatuses(_cts.Token);
                }
            }
        }
    }

    private async Task<EventDetailsViewModel> GetModel()
    {
        // First try to get the model from the state
        var modelState = floodEventState.Details;
        if (modelState is not null)
        {
            if (!string.IsNullOrWhiteSpace(SearchReference) && modelState.SearchReference.Equals(SearchReference, StringComparison.Ordinal))
            {
                return modelState;
            }
        }

        // Next try to get the saved search from the database
        if (!string.IsNullOrWhiteSpace(SearchReference))
        {
            var savedSearch = await commonRepository.GetFloodEvent(SavedSearchId: null, SearchReference, _cts.Token);
            if (savedSearch is not null)
            {
                return new EventDetailsViewModel(savedSearch);
            }
        }

        // When no model state, and no saved search in the database
        return new EventDetailsViewModel
        {
            SearchReference = SearchReference ?? "",
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

        var savedSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);
        if (savedSearch == null)
        {
            var messages = new ValidationMessageStore(EditContext);
            messages.Add(EditContext.Field(nameof(Model.SearchReference)), "The saved search could not be found. Please go back and try again.");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        var floodEvent = Model.ToFloodEvent(Model, floodEventState.Estimates);
        await commonRepository.SetSavedSearchTitle(savedSearch.Id, Model.SearchTitle, _cts.Token);
        await commonRepository.CreateOrUpdateFloodEvent(savedSearch.Id, floodEvent, _cts.Token);

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");
    }

    private async Task PopulateSearchTitle()
    {
        if (string.IsNullOrWhiteSpace(Model.SearchTitle))
        {
            // Try to get search criteria from state first, then fall back to database
            SearchResultsViewModel? searchCriteria = SearchResults;

            if (searchCriteria == null && !string.IsNullOrEmpty(SearchReference))
            {
                // Retrieve from database if not in state
                var savedSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);
                if (savedSearch != null)
                {
                    searchCriteria = new SearchResultsViewModel
                    {
                        FromDate = savedSearch.ResultsFrom.HasValue
                            ? new GdsDate(savedSearch.ResultsFrom.Value)
                            : new GdsDate(),
                        ToDate = savedSearch.ResultsTo.HasValue
                            ? new GdsDate(savedSearch.ResultsTo.Value)
                            : new GdsDate(),
                        ByPostcode = savedSearch.Postcode,
                        ByFloodSource = savedSearch.FloodSources.ToList(),
                        ByPolygon = savedSearch.Geom,
                    };
                }
            }

            if (searchCriteria != null)
            {
                var titleParts = new List<string>();

                // Get year and month from search dates
                DateTimeOffset? floodDate = searchCriteria.FromDate.DateUtc ?? searchCriteria.ToDate.DateUtc;
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
                if (searchCriteria.ByFloodSource?.Count > 0)
                {
                    var selectedFloodSourceIds = searchCriteria.ByFloodSource.ToHashSet();
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
            else
            {
                Model.SearchTitle = "[Add the date] [Add the location] (Add the flood type)";
            }
        }
    }

    private async Task GoToEstimates()
    {
        if (EditContext is null || !EditContext.Validate() || string.IsNullOrWhiteSpace(SearchReference))
        {
            return;
        }

        if (floodEventState.HasEstimates && string.Equals(floodEventState.Estimates?.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            var estimates = floodEventState.Estimates;
            estimates?.EstimatedTotalInternallyFlooded = Model.EstimatedTotalInternallyFlooded;
            estimates?.EstimatedTotalExternalOnly = Model.EstimatedTotalExternalOnly;
            estimates?.EstimatedDisruptedUtilities = Model.EstimatedDisruptedUtilities;
            floodEventState.SetEventEstimates(estimates);
        }
        else
        {
            var databaseModel = await commonRepository.GetFloodEvent(SavedSearchId: null, SearchReference, _cts.Token);
            EventEstimatesViewModel estimates;
            if (databaseModel != null)
            {
                estimates = new();

                estimates = estimates.ToModel(databaseModel);
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