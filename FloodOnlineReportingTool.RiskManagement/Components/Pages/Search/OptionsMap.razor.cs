using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class OptionsMap(
    ILogger<OptionsMap> logger,
    IGeoServerClient geoServerClient,
    ISavedSearchRepository savedSearchRepository,
    ISearchOptionsState searchOptionsState,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.OptionsMap.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
        SearchPages.Options.ToGdsBreadcrumb(),
    ];

    [Parameter]
    public string? SearchReference { get; set; }
    private string PreviousPageUrl => SearchReference is null ? SearchPages.Options.Url : $"{SearchPages.Options.Url}/{SearchReference}";

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext EditContext = default!;
    private EditContext regionContext = default!;
    private EditContext digitisingContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _canEdit;

    private string _searchTerm = string.Empty;
    private bool _isSearching = false;
    private IReadOnlyCollection<string> _parishResults = [];
    private string _selectedParish = string.Empty;
    private IReadOnlyCollection<string> _wardResults = [];
    private string _selectedWard = string.Empty;
    private string _regionSearchErrorMessage = string.Empty;
    private CancellationTokenSource? _searchCts;
    private ValidationMessageStore _messageStore = default!;

    private Guid _wardSelectKey = Guid.NewGuid();
    private Guid _parishSelectKey = Guid.NewGuid();

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        EditContext = new EditContext(Model);
        EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new(EditContext);

        // Reuse the same EditContext instance for all sections to keep validation state consistent
        regionContext = EditContext;
        digitisingContext = EditContext;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // 1. data from repository
            bool hasReferenceChanged = SearchReference is not null && !string.Equals(searchOptionsState.Options?.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase);
            if (hasReferenceChanged)
            {
                var savedSearch = await savedSearchRepository.Get(SearchReference!, _cts.Token);
                if (savedSearch is not null)
                {
                    Model.CopyFrom(savedSearch);
                    searchOptionsState.SetOptions(Model);
                }
            }

            // 2. State - for example returning from the options map page
            if (!hasReferenceChanged && searchOptionsState.Options is not null)
            {
                Model.CopyFrom(searchOptionsState.Options);
            }

            // 3. Page parameters
            Model.SearchReference = SearchReference;
            _canEdit = Model.OverrideLock || !Model.IsSavedSearchLocked;

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
            
            if (_searchCts != null)
            {
                await _searchCts.CancelAsync();
                _searchCts.Dispose();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(OptionsMap));
        }

        GC.SuppressFinalize(this);
    }

    private void SaveAndReturn()
    {
        searchOptionsState.SetOptions(Model);
        navigationManager.NavigateTo(PreviousPageUrl);
    }

    private async Task OnSearch()
    {
        _messageStore.Clear();

        if (!_canEdit)
        {
            var errorField = EditContext.Field(nameof(Model.SearchReference));
            _messageStore.Add(errorField, "This search is locked and cannot be modified");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        _regionSearchErrorMessage = string.Empty;
        // Clear results if search term is too short
        if (string.IsNullOrWhiteSpace(_searchTerm) || _searchTerm.Length < 3)
        {
            _regionSearchErrorMessage = "Please enter at least 3 characters to search";
            _parishResults = [];
            _wardResults = [];
            StateHasChanged();
            return;
        }

        await SearchAsync(_searchTerm);
    }

    private async Task SearchAsync(string searchTerm)
    {
        // Cancel any pending search
        if (_searchCts != null)
        {
            await _searchCts.CancelAsync();
        }
        _searchCts = new CancellationTokenSource();
        
        try
        {
            _isSearching = true;
            StateHasChanged();

            var authState = await (AuthenticationState ?? throw new InvalidOperationException("Authentication state is required"));
            var user = authState.User;

            _parishResults = await geoServerClient.SearchNameAsync("parish", searchTerm, user, _searchCts.Token);
            _wardResults = await geoServerClient.SearchNameAsync("ward", searchTerm, user, _searchCts.Token);
            if(_parishResults.Count == 0 && _wardResults.Count == 0)
            {
                _regionSearchErrorMessage = $"No results could be found for {searchTerm}";
            }

            logger.LogInformation("Parish search returned {Count} results for term: {SearchTerm}",
                _parishResults.Count, searchTerm);
            logger.LogInformation("Ward search returned {Count} results for term: {SearchTerm}",
                _wardResults.Count, searchTerm);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Search cancelled for term");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search for term: {SearchTerm}. Clearing parish and ward results.", searchTerm);
            _parishResults = [];
            _wardResults = [];
        }
        finally
        {
            //reset the selection key
            _parishSelectKey = Guid.NewGuid();
            _wardSelectKey = Guid.NewGuid();
            _isSearching = false;
            StateHasChanged();
        }
    }

    private async Task OnParishSelected()
    {
        var selectedParish = _selectedParish;
        if (!string.IsNullOrEmpty(selectedParish))
            await SelectRegion("parish", selectedParish);
    }

    private async Task OnWardSelected()
    {
        var selectedWard = _selectedWard;
        if (!string.IsNullOrEmpty(selectedWard))
            await SelectRegion("ward", selectedWard);
    }

    private async Task SelectRegion(string source, string selected)
    {
        logger.LogInformation("User selected {Source}: {Value}", source, selected);

        var authState = await AuthenticationState!;
        var user = authState.User;

        var geometry = await geoServerClient.GetGeometryFromSourceBySearch(source, selected, user, _cts.Token);

        if (geometry is Polygon polygon)
        {
            geometry = new MultiPolygon(new Polygon[] { polygon });
        }

        if (geometry is MultiPolygon multiPolygon)
        {
            var merged = MergePolygon(multiPolygon);
            if (merged is not null)
                Model.ByPolygon = merged;
        }
        else if (geometry is not null)
        {
            logger.LogWarning("Geometry for {Source} {Value} is not a MultiPolygon as expected. Type: {GeometryType}",
                source, selected, geometry.GeometryType);
        }

        StateHasChanged();
    }

    private MultiPolygon? MergePolygon(MultiPolygon incoming)
    {
        const int srid = 27700;

        MultiPolygon? result;

        if (Model.ByPolygon is null)
        {
            result = incoming;
        }
        else
        {
            var unionGeom = Model.ByPolygon.Union(incoming);
            result = unionGeom switch
            {
                MultiPolygon mp => mp,
                Polygon p => new MultiPolygon([p]),
                _ => null
            };

            if (result is null)
            {
                logger.LogWarning("Union result is unexpected geometry type: {GeometryType}", unionGeom.GeometryType);
                return null;
            }
        }

        result.SRID = srid;
        return result;
    }

    private void ResetGeom()
    {
        Model.ByPolygon = null;
        StateHasChanged();
    }

}