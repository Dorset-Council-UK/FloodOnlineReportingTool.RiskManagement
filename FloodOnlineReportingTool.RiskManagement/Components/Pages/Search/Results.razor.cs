using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Identity.Web;
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(ILogger<Results> logger,
                             ICommonRepository commonRepository,
                             IReportStatusClient reportStatusClient,
                             ISearchResultsState searchResultsState,
                             IWebHostEnvironment environment,
                             MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
                             NavigationManager navigationManager) : IAsyncDisposable
{
    private EditContext _editContext = null!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel SearchResultsViewModel { get; set; } = default!;
    public PagedResult<SearchResultFloodReport> SearchResults { get; set; } = default!;
    private SearchState? _searchState;

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;
    public GridItemsProvider<SearchResultFloodReport>? SearchResultsProvider { get; private set; }

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


        var options = searchResultsState.Options;
        if (options is null || !string.Equals(options.ByReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            var savedSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);

            if (savedSearch == null)
            {
                // Failure - No saved search found for reference
                _isLoading = false;
                StateHasChanged();
                return;
            }
            // Convert Entity → ViewModel
            SearchResultsViewModel = savedSearch.SavedSearchToResultsView();
                
            // Persist in state for page navigation
            searchResultsState.SetResultOptions(SearchResultsViewModel);
        }
        else
        {
            SearchResultsViewModel = options;
        }
        //execute the API search
        await Search();
        SearchResultsProvider = ProvideSearchResults;
        _isLoading = false;
        StateHasChanged();
    }

    private async ValueTask<GridItemsProviderResult<SearchResultFloodReport>> ProvideSearchResults(
        GridItemsProviderRequest<SearchResultFloodReport> request)
    {
        try
        {
            var pageSize = request.Count ?? SearchResultsViewModel.PagingSize;
            var pageNumber = (request.StartIndex / pageSize) + 1;

            var result = await reportStatusClient.SearchFloodReports(
                SearchResultsViewModel.FromDate.DateUtc,
                SearchResultsViewModel.ToDate.DateUtc,
                SearchResultsViewModel.ByFloodSource,
                null, null, SearchResultsViewModel.ByPostcode,
                SearchResultsViewModel.ByPolygon,
                pageNumber, pageSize,
                request.CancellationToken);

            List<SearchResultFloodReport> items = result?.Results.ToList() ?? [];
            return GridItemsProviderResult.From(items, result?.TotalCount ?? 0);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while fetching flood reports for table view");
            consentHandler.HandleException(ex);
            return GridItemsProviderResult.From<SearchResultFloodReport>([], 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching flood reports for table view");
            return GridItemsProviderResult.From<SearchResultFloodReport>([], 0);
        }
    }

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
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
                                                                        pageNumber);

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
            var allResults = new List<SearchResultFloodReport>();
            const int pageSize = 100;
            var totalPages = (int)Math.Ceiling((double)SearchResults.TotalCount / pageSize);

            for (var page = 1; page <= totalPages; page++)
            {
                var result = await reportStatusClient.SearchFloodReports(
                    SearchResultsViewModel.FromDate.DateUtc,
                    SearchResultsViewModel.ToDate.DateUtc,
                    SearchResultsViewModel.ByFloodSource,
                    null, null, 
                    SearchResultsViewModel.ByPostcode, 
                    SearchResultsViewModel.ByPolygon,
                    page, pageSize,
                    _cts.Token);

                if (result?.Results is not null)
                {
                    allResults.AddRange(result.Results);
                }
            }

            var markers = allResults
                .Where(r => r.Latitude != 0 || r.Longitude != 0)
                .Select(r => new MapMarkerData(
                    r.Id.ToString(),
                    r.Latitude,
                    r.Longitude,
                    BuildPopupHtml(r)))
                .ToList();

            if (_miniMapRef is not null)
            {
                await _miniMapRef.UpdateMarkersAsync(markers);
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

    private static string BuildPopupHtml(SearchResultFloodReport report)
    {
        var sb = new StringBuilder();
        sb.Append($"<div class=\"govuk-body-s govuk-!-margin-bottom-0\"><strong>{report.Id}</strong><br/>");

        if (report.ImpactStartUtc is not null)
        {
            sb.Append($"<strong>Flooding started:</strong> {report.ImpactStartUtc.Value:d MMMM yyyy}<br/>");
        }
        var sources = report.Sources.Count > 0 ? string.Join(", ", report.Sources.Select(s => s.Name)) : "No flood sources identified";
        sb.Append($"<strong>Sources:</strong> {sources}<br/>");
        sb.Append($"<strong>Created:</strong> {report.CreatedUtc:d MMMM yyyy}<br/>");

        if (report.UpdatedUtc is not null)
        {
            sb.Append($"<strong>Updated:</strong> {report.UpdatedUtc.Value:d MMMM yyyy}<br/>");
        }

        sb.Append($"<a href=\"flood-report/details/{report.Id}\" class=\"govuk-link\">View details</a>");
        sb.Append("</div>");

        return sb.ToString();
    }

}