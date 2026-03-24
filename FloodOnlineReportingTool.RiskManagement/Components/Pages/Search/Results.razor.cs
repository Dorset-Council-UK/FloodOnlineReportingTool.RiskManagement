using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Identity.Web;

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
    private ValidationMessageStore _messageStore = null!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel SearchResultsViewModel { get; set; } = default!;
    public PagedResult<SearchResultFloodReport> SearchResults { get; set; } = default!;
    private SearchState? _searchState;

    [SupplyParameterFromQuery(Name = "ref")]
    public required string SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        SearchResultsViewModel ??= new();
        _editContext = new(SearchResultsViewModel);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Options));
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

                //execute the API search
                await Search();
            }
            else
            {
                SearchResultsViewModel = options;
            }

            _isLoading = false;
            StateHasChanged();
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
            SearchResults = await reportStatusClient.SearchFloodReports(SearchResultsViewModel.FromDate.DateUtc, SearchResultsViewModel.ToDate.DateUtc, SearchResultsViewModel.ByFloodSource, null, null, null, pageNumber);

            _searchState = SearchResults is null || SearchResults.TotalCount == 0
                ? SearchState.NoResults
                : SearchState.ResultsFound;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while searching flood locations");
            var searchAgain = true;
            consentHandler.HandleException(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while searching flood locations");

            _searchState = SearchState.Error;

            var formErrorField = FieldIdentifier.Create(() => SearchResultsViewModel.PageNumber);
            _messageStore.Add(formErrorField,
            [
                "There was a problem searching for flood locations",
                "Please try again and if the problem persists, contact support.",
            ]);
            if (environment.IsDevelopment())
            {
                _messageStore.Add(formErrorField, ex.Message);
            }
            _editContext.NotifyValidationStateChanged();
        }
    }

    private bool ValidSearch()
    {
        _messageStore.Clear();
        var valid = _editContext.Validate();
        logger.LogDebug("Search flood locations form valid: {IsValid}", valid);
        return valid;
    }

    private async Task OnPageChanged(int newPageNumber)
    {
        SearchResultsViewModel.PageNumber = newPageNumber;
        if (!ValidSearch())
        {
            _editContext.NotifyValidationStateChanged();
            return;
        }
        await Search();
    }

}