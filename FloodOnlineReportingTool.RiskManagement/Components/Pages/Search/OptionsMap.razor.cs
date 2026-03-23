using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
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

    private EditContext searchContext = default!;
    private EditContext regionContext = default!;
    private EditContext digitisingContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private string _searchTerm = string.Empty;
    private bool _isSearching = false;
    private IReadOnlyCollection<GdsOptionItem<string>> _parishResults = [];
    private IReadOnlyCollection<GdsOptionItem<string>> _wardResults = [];
    private string _regionSearchErrorMessage = string.Empty;
    private CancellationTokenSource? _searchCts;
    private int _selectKey = 0;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new EditContext(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        // Reuse the same EditContext instance for all sections to keep validation state consistent
        regionContext = searchContext;
        digitisingContext = searchContext;
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
                Model.FromDate.DayText = options.FromDate.DayText;
                Model.FromDate.MonthText = options.FromDate.MonthText;
                Model.FromDate.YearText = options.FromDate.YearText;
                Model.ToDate.DayText = options.ToDate.DayText;
                Model.ToDate.MonthText = options.ToDate.MonthText;
                Model.ToDate.YearText = options.ToDate.YearText;
                Model.ByPlaceName = options.ByPlaceName;
                Model.ByPersonsName = options.ByPersonsName;
                Model.ByFloodSource = options.ByFloodSource;
                Model.ByPolygon = options.ByPolygon;
                Model.ShowResultsAs = options.ShowResultsAs;
                Model.BufferText = options.BufferText;
                Model.Buffer = options.Buffer;
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
            _selectKey++;
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

            var parishSearchResults = await geoServerClient.SearchNameAsync("parish", searchTerm, user, _searchCts.Token);
            var wardSearchResults = await geoServerClient.SearchNameAsync("ward", searchTerm, user, _searchCts.Token);
            if(parishSearchResults.Count + wardSearchResults.Count == 0)
            {
                _regionSearchErrorMessage = $"No results could be found for {searchTerm}";
            }
            _parishResults = CreateGdsOptionItems(parishSearchResults);
            _wardResults = CreateGdsOptionItems(wardSearchResults);
            _selectKey++;

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
            _isSearching = false;
            StateHasChanged();
        }
    }

    private async Task OnParishSelected(ChangeEventArgs e)
    {
        var selectedParish = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(selectedParish))
        {
            logger.LogInformation("User selected parish: {Parish}", selectedParish);

            var authState = await AuthenticationState!;
            var user = authState.User;

            // Store selection in your model
            var geometry = await geoServerClient.GetGeometryFromSourceBySearch("parish", selectedParish, user, _cts.Token);

            if (geometry != null)
            {
                if (geometry is MultiPolygon multiPolygon)
                {
                    if (Model.ByPolygon != null)
                    {
                        var unionGeom = Model.ByPolygon.Union(geometry);
                        if (unionGeom is MultiPolygon newMultiPolygon)
                        {
                            Model.ByPolygon = newMultiPolygon;

                        } else if (unionGeom is Polygon singlePolygon)
                        {
                            Model.ByPolygon = new MultiPolygon(new[] { singlePolygon });
                        } else
                        {
                            logger.LogWarning("Union result is unexpected geometry type: {GeometryType}", unionGeom.GeometryType);
                        }
                    }
                    else
                    {
                        Model.ByPolygon = multiPolygon;
                    }
                } else
                {
                    logger.LogWarning("Geometry for parish {Parish} is not a MultiPolygon as expected. Type: {GeometryType}",
                        selectedParish, geometry.GeometryType);
                }
            }
            StateHasChanged();
        }
    }

    private async Task OnWardSelected(ChangeEventArgs e)
    {
        var selectedWard = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(selectedWard))
        {
            logger.LogInformation("User selected ward: {Ward}", selectedWard);

            var authState = await AuthenticationState!;
            var user = authState.User;

            // Store selection in your model
            var geometry = await geoServerClient.GetGeometryFromSourceBySearch("ward", selectedWard, user, _cts.Token);

            if (geometry != null)
            {
                if (geometry is MultiPolygon multiPolygon)
                {
                    if (Model.ByPolygon != null)
                    {
                        var unionGeom = Model.ByPolygon.Union(geometry);
                        if (unionGeom is MultiPolygon newMultiPolygon)
                        {
                            Model.ByPolygon = newMultiPolygon;

                        }
                        else if (unionGeom is Polygon singlePolygon)
                        {
                            Model.ByPolygon = new MultiPolygon(new[] { singlePolygon });
                        }
                        else
                        {
                            logger.LogWarning("Union result is unexpected geometry type: {GeometryType}", unionGeom.GeometryType);
                        }
                    }
                    else
                    {
                        Model.ByPolygon = multiPolygon;
                    }
                }
                else
                {
                    logger.LogWarning("Geometry for ward {Ward} is not a MultiPolygon as expected. Type: {GeometryType}",
                        selectedWard, geometry.GeometryType);
                }
            }
            StateHasChanged();
        }
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