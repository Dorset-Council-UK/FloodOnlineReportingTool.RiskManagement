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
    private EditContext EditContext = default!;
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
        Model.SearchReference = SearchReference;
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
                EditContext = new(Model);
                EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            }
            else
            {
                var databaseModel = await commonRepository.GetFloodEvent(null, SearchReference, _cts.Token);
                if (databaseModel != null)
                {
                    Model = Model.ToModel(databaseModel);
                    EditContext = new(Model);
                    EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                }

                // Clear the existing state if the search reference does not match, to avoid displaying stale data
                Model.SearchReference = SearchReference ?? "";
                floodEventState.SetEventDetails(Model);

                // Pre-populate the search title if it's empty
                await PopulateSearchTitle();
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Details));
        }
    }

    private async Task OnValidSubmit()
    {
        // Save the record
        if (SearchReference == null)
        {
            return;
        }

        var currentSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);
        if (currentSearch == null)
        {
            return;
        }

        var floodEvent = Model.ToFloodEvent(Model, floodEventState.Estimates);
        await commonRepository.SetSavedSearchTitle(currentSearch.Id, Model.SearchTitle, _cts.Token);
        await commonRepository.CreateOrUpdateFloodEvent(currentSearch.Id, floodEvent, _cts.Token);

        // Clear the model
        floodEventState.SetEventDetails(null);
        floodEventState.SetEventEstimates(null);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
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
                        ByPolygon = savedSearch.Geom
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
                    titleParts.Add(floodDate.Value.Year.ToString());
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
                    Model.SearchTitle = string.Join(" ", titleParts);
                }
            }
        }
    }

    private async Task GoToEstimates()
    {
        if (EditContext.Validate())
        {
            var existingDetails = floodEventState.Details;
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
                var databaseModel = await commonRepository.GetFloodEvent(null, SearchReference, _cts.Token);
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
                        SearchReference = SearchReference
                    };
                }

                floodEventState.SetEventEstimates(estimates);
            }

            navigationManager.NavigateTo($"{EventPages.Estimates.Url}/{SearchReference}");
        }
        StateHasChanged();
        return;
    }
}