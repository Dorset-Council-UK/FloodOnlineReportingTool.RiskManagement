using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NetTopologySuite.Geometries;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class OptionsMap(
    ILogger<OptionsMap> logger,
    IGeoServerClient geoServerClient,
    NavigationManager navigationManager,
    ISearchOptionsState searchOptionsState
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.OptionsMap.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
        SearchPages.Options.ToGdsBreadcrumb(),
    ];

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext EditContext = default!;
    private EditContext regionContext = default!;
    private EditContext digitisingContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private string _searchTerm = string.Empty;
    private bool _isSearching = false;
    private IReadOnlyCollection<string> _parishResults = [];
    private string _selectedParish = string.Empty;
    private IReadOnlyCollection<string> _wardResults = [];
    private string _selectedWard = string.Empty;
    private string _regionSearchErrorMessage = string.Empty;
    private CancellationTokenSource? _searchCts;

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
        // Reuse the same EditContext instance for all sections to keep validation state consistent
        regionContext = EditContext;
        digitisingContext = EditContext;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var options = searchOptionsState.Options;
            // GdsInputDate caches the GdsDate object reference on first render, so the existing
            // objects must be mutated in-place rather than replaced to keep the component in sync.
            if (options is not null)
            {
                Model = options;
                EditContext = new EditContext(Model);
                EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                // Reuse the same EditContext instance for all sections to keep validation state consistent
                regionContext = EditContext;
                digitisingContext = EditContext;
                Model.FromDate.DayText = options.FromDate.DayText;
                Model.FromDate.MonthText = options.FromDate.MonthText;
                Model.FromDate.YearText = options.FromDate.YearText;
                Model.ToDate.DayText = options.ToDate.DayText;
                Model.ToDate.MonthText = options.ToDate.MonthText;
                Model.ToDate.YearText = options.ToDate.YearText;
                Model.ByPostcode = options.ByPostcode;
                Model.ByPersonsName = options.ByPersonsName;
                Model.ByFloodSource = options.ByFloodSource;
                Model.ByPolygon = options.ByPolygon;
            }

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
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation; no action required.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(OptionsMap));
        }
    }

    private void SaveAndReturn()
    {
        searchOptionsState.SetOptions(Model);
        navigationManager.NavigateTo(SearchPages.Options.Url);
    }

    private async Task OnSearch()
    {
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
        _searchCts?.Cancel();
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

    private static List<GdsOptionItem<string>> CreateGdsOptionItems(IEnumerable<string> results)
    {
        var optionItems = new List<GdsOptionItem<string>>
        {
            new("0".AsSpan(), "Select an option", string.Empty, false)
        };
        optionItems.AddRange(results.Select((o, idx) =>
            new GdsOptionItem<string>((idx + 1).ToString(CultureInfo.InvariantCulture).AsSpan(), o, o, false)));

        return optionItems;
    }

    private void ResetGeom()
    {
        Model.ByPolygon = null;
        StateHasChanged();
    }

}