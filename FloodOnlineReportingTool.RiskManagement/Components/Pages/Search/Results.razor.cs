using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
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
                             IReferenceSearchService referenceSearchService,
                             ISearchResultsState searchResultsState,
                             IWebHostEnvironment environment,
                             MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
                             NavigationManager navigationManager) : IAsyncDisposable
{
    private EditContext _editContext = null!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel SearchResultsViewModel { get; set; } = default!;
    public PagedResult<FloodResultItem> SearchResults { get; set; } = default!;
    private SearchState? _searchState;
    private IList<ReportStatus> _reportStatuses = [];

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;
    public GridItemsProvider<FloodResultItem>? SearchResultsProvider { get; private set; }

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

        var options = searchResultsState.Options;
        if (options is null || !string.Equals(options.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _gettingToken = false;
                var foundEntity = await referenceSearchService.FindByReference(SearchReference, _cts.Token);

                if (foundEntity is null || !foundEntity.IsSuccess)
                {
                    _isLoading = false;
                    StateHasChanged();
                    return;
                }

                if (foundEntity.ResultModel?.RedirectType == RecordTypes.Search && foundEntity.ResultModel?.Id.HasValue == true)
                {
                    var savedSearch = await commonRepository.GetSavedSearch((Guid)foundEntity.ResultModel.Id, _cts.Token);
                    SearchResultsViewModel = savedSearch?.SavedSearchToResultsView() ?? new SearchResultsViewModel();
                    SearchResultsViewModel.SearchReference = SearchReference;
                    searchResultsState.SetResultOptions(SearchResultsViewModel);
                }
                else
                {
                    // handle other redirect types
                    if(foundEntity.ResultModel?.RedirectType == RecordTypes.Report)
                    {
                        var nextPage = $"flood-report/details/{foundEntity.ResultModel.Reference}";
                        navigationManager.NavigateTo(nextPage);
                    } else if (foundEntity.ResultModel?.RedirectType == RecordTypes.Event)
                    {
                        var nextPage = $"{EventPages.Details.Url}/{foundEntity.ResultModel.Reference}";
                        navigationManager.NavigateTo(nextPage);
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
        }
        else
        {
            SearchResultsViewModel = options;
        }

        // Ensure the context is linked to the (possibly new) view model and messages cleared
        _editContext = new(SearchResultsViewModel);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new ValidationMessageStore(_editContext);

        //execute the API search
        await Search();
        SearchResultsProvider = ProvideSearchResults;
        _isLoading = false;
        StateHasChanged();
    }

    private async ValueTask<GridItemsProviderResult<FloodResultItem>> ProvideSearchResults(
        GridItemsProviderRequest<FloodResultItem> request)
    {
        try
        {
            var pageSize = request.Count ?? SearchResultsViewModel.PagingSize;
            var pageNumber = (request.StartIndex / pageSize) + 1;

            var rawResult = await reportStatusClient.SearchFloodReports(
                SearchResultsViewModel.FromDate.DateUtc,
                SearchResultsViewModel.ToDate.DateUtc,
                SearchResultsViewModel.ByFloodSource,
                null, null, SearchResultsViewModel.ByPostcode,
                SearchResultsViewModel.ByPolygon,
                pageNumber, pageSize,
                request.CancellationToken);
            var result = rawResult.ToFloodResultItem(_reportStatuses);

            List<FloodResultItem> items = result?.Results.ToList() ?? [];
            return GridItemsProviderResult.From(items, result?.TotalCount ?? 0);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while fetching flood reports for table view");
            consentHandler.HandleException(ex);
            return GridItemsProviderResult.From<FloodResultItem>([], 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching flood reports for table view");
            return GridItemsProviderResult.From<FloodResultItem>([], 0);
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
            var rawResult = await reportStatusClient.SearchFloodReports(SearchResultsViewModel.FromDate.DateUtc,
                                                                        SearchResultsViewModel.ToDate.DateUtc,
                                                                        SearchResultsViewModel.ByFloodSource,
                                                                        null,
                                                                        null,
                                                                        SearchResultsViewModel.ByPostcode,
                                                                        SearchResultsViewModel.ByPolygon,
                                                                        pageNumber);
            SearchResults = rawResult.ToFloodResultItem(_reportStatuses);

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
                    allResults.AddRange(result.Results.ToList());
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

    private static string BuildPopupHtml(FloodResultItem report)
    {
        var sb = new StringBuilder();
        sb.Append($"<div class=\"govuk-body-s govuk-!-margin-bottom-0\"><strong>{report.Reference}</strong><br/>");
        sb.Append($"<strong>Report status:</strong> {report.RecordStatus}<br/>");

        if (report.ImpactStartUtc is not null)
        {
            sb.Append($"<strong>Flooding started:</strong> {report.ImpactStartUtc.Value:d MMMM yyyy}<br/>");
        }
        var sources = report.Causes.Count > 0 ? string.Join(", ", report.Causes.Select(s => s.Name)) : "No flooding sources identified";
        sb.Append($"<strong>Sources:</strong> {sources}<br/>");
        sb.Append($"<strong>Created:</strong> {report.CreatedUtc:d MMMM yyyy}<br/>");

        if (report.UpdatedUtc is not null)
        {
            sb.Append($"<strong>Updated:</strong> {report.UpdatedUtc.Value:d MMMM yyyy}<br/>");
        }

        sb.Append($"<a href=\"{report.ViewUri}\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"govuk-link\">View details</a>");
        sb.Append("</div>");

        return sb.ToString();
    }

}