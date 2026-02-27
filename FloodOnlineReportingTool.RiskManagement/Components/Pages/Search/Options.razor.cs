using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    ICommonRepository commonRepository,
    IGeoServerClient geoServerClient,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.Options.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
        SearchPages.Options.ToGdsBreadcrumb(),
    ];

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext searchContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private IReadOnlyCollection<GdsOptionItem<Guid>> _floodSourceOptions = [];
    private string _parishSearchTerm = string.Empty;
    private bool _isSearchingParishes = false;
    private IReadOnlyCollection<GdsOptionItem<string>> _parishResults = new List<GdsOptionItem<string>>();
    private CancellationTokenSource? _searchCts;
    private System.Timers.Timer? _searchDebounceTimer;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var options = await GetSearchOptions();
            Model.ToDate = options.ToDate;
            Model.FromDate = options.FromDate;
            Model.SearchType = options.SearchType;
            Model.ByPostcode = options.ByPostcode;
            Model.ByName = options.ByName;
            Model.EastingText = options.EastingText;
            Model.NorthingText = options.NorthingText;
            Model.BufferText = options.BufferText;
            Model.Buffer = options.Buffer;

            _floodSourceOptions = await CreateFloodSourceOptions(Model.FloodProblemIds);
            _isLoading = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();

            _searchDebounceTimer?.Dispose();
            
            if (_searchCts != null)
            {
                await _searchCts.CancelAsync();
                _searchCts.Dispose();
            }
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

    private async Task OnSubmit()
    {
        //Store the search options in session storage for retrieval on the next page
        await protectedSessionStorage.SetAsync(SessionConstants.SearchOptions, Model);

        // Go to the next page or pass back to the summary
        var nextPage = GetNextPage();
        var nextPageUrl = nextPage.Url;
        navigationManager.NavigateTo(nextPageUrl);
    }

    private PageInfo GetNextPage()
    {
        // Add filters for specific options
        switch (Model.SearchType)
        {
            case FloodLocationSearchType.All:
                return SearchPages.Results;
            case FloodLocationSearchType.WithFloodReports:
                return SearchPages.Results;
            case FloodLocationSearchType.ByLocation:
                return SearchPages.Location;

            default:
                // Failure, just return the current page
                return SearchPages.Options;
        }
    }

    private async Task<SearchOptionsViewModel> GetSearchOptions()
    {
        var data = await protectedSessionStorage.GetAsync<SearchOptionsViewModel>(SessionConstants.SearchOptions);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Search options were not found in the protected storage.");
        return new SearchOptionsViewModel();
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "flood-source";
        var floodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodProblem.Id);

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected);
    }

    private async Task OnParishSearchChanged()
    {      
        // Cancel any pending search
        _searchDebounceTimer?.Stop();
        _searchCts?.Cancel();
        
        // Clear results if search term is too short
        if (string.IsNullOrWhiteSpace(_parishSearchTerm) || _parishSearchTerm.Length < 3)
        {
            _parishResults = new List<GdsOptionItem<string>>();
            StateHasChanged();
            return;
        }
        
        // Debounce: wait 400ms after user stops typing
        _searchDebounceTimer = new System.Timers.Timer(400);
        _searchDebounceTimer.Elapsed += async (s, e) =>
        {
            await InvokeAsync(async () =>
            {
                await SearchParishesAsync(_parishSearchTerm);
            });
        };
        _searchDebounceTimer.AutoReset = false;
        _searchDebounceTimer.Start();
    }

    private async Task SearchParishesAsync(string searchTerm)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        
        try
        {
            _isSearchingParishes = true;
            StateHasChanged();
            
            _parishResults = await geoServerClient.SearchParishNameAsync(searchTerm, _searchCts.Token);
            
            logger.LogInformation("Parish search returned {Count} results for term: {SearchTerm}", 
                _parishResults.Count, searchTerm);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Parish search cancelled for term: {SearchTerm}", searchTerm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search parishes for term: {SearchTerm}", searchTerm);
            _parishResults = new List<GdsOptionItem<string>>();
        }
        finally
        {
            _isSearchingParishes = false;
            StateHasChanged();
        }
    }

    private void OnParishSelected(ChangeEventArgs e)
    {
        var selectedParish = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(selectedParish))
        {
            logger.LogInformation("User selected parish: {Parish}", selectedParish);
            // Store selection in your model
            // Model.SelectedParish = selectedParish;
        }
    }
}