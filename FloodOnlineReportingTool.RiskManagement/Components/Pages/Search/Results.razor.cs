using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;
using System.Net;
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(
    ILogger<Results> logger,
    ICommonRepository commonRepository,
    IReportStatusClient reportStatusClient,
    IReferenceSearchService referenceSearchService,
    ISavedSearchRepository savedSearchRepository,
    IWebHostEnvironment environment,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private EditContext _editContext = null!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel SearchResultsViewModel { get; set; } = default!;
    public SearchFloodReportPagedResult<SearchResultFloodReport> SearchResults { get; set; } = default!;
    private SearchState? _searchState;
    private IList<ReportStatus> _reportStatuses = [];

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;
    public SearchFloodReportPagedResult<FloodReportTableRow>? SearchResultsGrouped { get; private set; }

    private MiniMap? _miniMapRef;
    private bool _isLoadingMapData;
    private bool _mapDataLoaded;
    private bool _mapResultsTooMany;
    private bool _gettingToken;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        SearchResultsViewModel ??= new();
        _editContext = new(SearchResultsViewModel);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new ValidationMessageStore(_editContext);
        _reportStatuses = await commonRepository.GetReportStatuses(_cts.Token);
        _gettingToken = true;
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

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(SearchReference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            navigationManager.NavigateTo(SearchPages.Options.Url);
            return;
        }

        // Always load the search parameters fresh from the saved search database,
        // which is the source of truth. This avoids showing stale parameters when an
        // existing saved search is edited and re-run under the same reference.
        try
        {
            _gettingToken = false;
            var searchRedirect = await referenceSearchService.FindByReference(SearchReference, _cts.Token);

            if (searchRedirect is null)
            {
                _isLoading = false;
                StateHasChanged();
                return;
            }

            if (searchRedirect.RedirectType == RecordTypes.Search && searchRedirect.Id.HasValue)
            {
                Guid savedSearchId = searchRedirect.Id.Value;
                var savedSearch = await savedSearchRepository.Get(savedSearchId, _cts.Token);
                SearchResultsViewModel = savedSearch?.ToSearchResultsViewModel() ?? new SearchResultsViewModel();
                SearchResultsViewModel.SearchReference = SearchReference;
            }
            else
            {
                // handle other redirect types
                if (searchRedirect.RedirectType == RecordTypes.Report)
                {
                    var nextPage = $"{ReportPages.Details.Url}/{searchRedirect.Reference}";
                    navigationManager.NavigateTo(nextPage);
                    return;
                }
                else if (searchRedirect.RedirectType == RecordTypes.Event)
                {
                    var nextPage = $"{EventPages.View.Url}/{searchRedirect.Reference}";
                    navigationManager.NavigateTo(nextPage);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // Handles MsalUiRequiredException / MicrosoftIdentityWebChallengeUserException.
            // consentHandler.HandleException will trigger a redirect to re-authenticate
            // or prompt for incremental consent, then return the user to this page.
            consentHandler.HandleException(ex);
        }

        // Ensure the context is linked to the (possibly new) view model and messages cleared
        _editContext = new(SearchResultsViewModel);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new ValidationMessageStore(_editContext);

        //execute the API search
        await Search();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task Search()
    {
        _searchState = SearchState.Searching;
        StateHasChanged();

        try
        {
            var pageNumber = SearchResultsViewModel.PageNumber ?? 1;
            SearchResults = await reportStatusClient.SearchFloodReports(SearchResultsViewModel.FromDate.DateUtc,
                                                                        SearchResultsViewModel.ToDate.DateUtc,
                                                                        SearchResultsViewModel.ByFloodSource,
                                                                        null,
                                                                        null,
                                                                        SearchResultsViewModel.ByPostcode,
                                                                        SearchResultsViewModel.ByPolygon,
                                                                        pageNumber,
                                                                        ct: _cts.Token);
            SearchResultsGrouped = SearchResults.ToFloodReportTableRows(_reportStatuses);

            _searchState = SearchResults is null || SearchResults.TotalCount == 0
                ? SearchState.NoResults
                : SearchState.ResultsFound;

            _gettingToken = false;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _gettingToken = true;
            StateHasChanged();
            logger.LogWarning(ex, "Consent required while searching flood reports");
            consentHandler.HandleException(ex);
        }
        catch (Exception ex)
        {
            _gettingToken = false;
            logger.LogError(ex, "Error while searching flood reports");

            _searchState = SearchState.Error;

            var formErrorField = FieldIdentifier.Create(() => SearchResultsViewModel.PageNumber);
            _messageStore.Add(formErrorField,
            [
                "There was a problem searching for flood reports",
                "Please try again and if the problem persists, contact support.",
            ]);
            if (environment.IsDevelopment())
            {
                _messageStore.Add(formErrorField, ex.Message);
            }
            _editContext.NotifyValidationStateChanged();
        }
    }

    private async Task OnPageChanged(int newPageNumber)
    {
        SearchResultsViewModel.PageNumber = newPageNumber;
        await Search();
    }

    private async Task OnMapBecameVisible()
    {
        await LoadMapDataAsync();
    }

    private async Task LoadMapDataAsync()
    {
        if (_mapDataLoaded || _isLoadingMapData || SearchResults is null)
        {
            return;
        }

        const int maxMapResults = 1000;
        if (SearchResults.TotalCount > maxMapResults)
        {
            _mapResultsTooMany = true;
            StateHasChanged();
            return;
        }

        _isLoadingMapData = true;
        StateHasChanged();

        try
        {
            var allResults = new List<FloodResultItem>();
            const int pageSize = 100;
            var totalPages = (int)Math.Ceiling((double)SearchResults.TotalCount / pageSize);

            for (var page = 1; page <= totalPages; page++)
            {
                var rawResult = await reportStatusClient.SearchFloodReports(
                    SearchResultsViewModel.FromDate.DateUtc,
                    SearchResultsViewModel.ToDate.DateUtc,
                    SearchResultsViewModel.ByFloodSource,
                    null, null, 
                    SearchResultsViewModel.ByPostcode, 
                    SearchResultsViewModel.ByPolygon,
                    page, pageSize,
                    _cts.Token);
                var result = rawResult.ToFloodResultItem(_reportStatuses);

                if (result?.Results is not null)
                {
                    allResults.AddRange([.. result.Results]);
                }
            }

            // FORE TESTING: duplicate the first marker to test popup navigation; comment out to disable test
            //if (markers.Count > 0)
            //{
            //    var firstMarker = markers[0];
            //    for (var duplicate = 1; duplicate <= 2; duplicate++)
            //    {
            //        markers.Add(firstMarker with
            //        {
            //            Id = $"{firstMarker.Id}-test-duplicate-{duplicate}",
            //            PopupHtml = $"<p><strong>Temporary test duplicate {duplicate}</strong></p>{firstMarker.PopupHtml}"
            //        });
            //    }
            //}

            if (_miniMapRef is not null)
            {
                await _miniMapRef.UpdateMarkersAsync(allResults.ToMarkers());
                //Add the area used as part of the search (if there is one).
                if (SearchResultsViewModel.ByPolygon is not null)
                    await _miniMapRef.UpdateGeometryAsync(SearchResultsViewModel.ByPolygon);
            }
            _mapDataLoaded = true;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while fetching map data");
            consentHandler.HandleException(ex);
        }
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation; no action required.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching flood report data for map view");
        }
        finally
        {
            _isLoadingMapData = false;
            StateHasChanged();
        }
    }
}