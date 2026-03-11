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
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class OptionsMap(
    ILogger<OptionsMap> logger,
    ICommonRepository commonRepository,
    IGeoServerClient geoServerClient,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage,
    IConfiguration configuration,
    ISearchOptionsState searchOptionsState
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
    private EditContext regionContext = default!;
    private EditContext digitisingContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private IReadOnlyCollection<GdsOptionItem<Guid>> _floodSourceOptions = [];
    private string _searchTerm = string.Empty;
    private bool _isSearching = false;
    private IReadOnlyCollection<GdsOptionItem<string>> _parishResults = new List<GdsOptionItem<string>>();
    private IReadOnlyCollection<GdsOptionItem<string>> _wardResults = new List<GdsOptionItem<string>>();
    private CancellationTokenSource? _searchCts;
    private int _selectKey = 0;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new EditContext(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        // Reuse the same EditContext instance for all sections to keep validation state consistent
        regionContext = searchContext;
        digitisingContext = searchContext;
        _floodSourceOptions = await CreateFloodSourceOptions(Model.ByFloodCause);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Prefer circuit-scoped in-memory state (handles large geometries without browser storage limits),
            // falling back to session storage for the initial load.
            var options = searchOptionsState.HasOptions
                ? searchOptionsState.Options!
                : await GetSearchOptions();
            Model.ToDate = options.ToDate;
            Model.FromDate = options.FromDate;
            Model.ByPlaceName = options.ByPlaceName;
            Model.ByPersonsName = options.ByPersonsName;
            Model.ByFloodCause = options.ByFloodCause;
            Model.ByPolygon = options.ByPolygon;
            Model.ShowResultsAs = options.ShowResultsAs;
            Model.BufferText = options.BufferText;
            Model.Buffer = options.Buffer;

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

    private async Task OnSearch()
    {      
        // Clear results if search term is too short
        if (string.IsNullOrWhiteSpace(_searchTerm) || _searchTerm.Length < 3)
        {
            _parishResults = new List<GdsOptionItem<string>>();
            _wardResults = new List<GdsOptionItem<string>>();
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

            _parishResults = await geoServerClient.SearchNameAsync("parish", searchTerm, user, _searchCts.Token);
            _wardResults = await geoServerClient.SearchNameAsync("ward", searchTerm, user, _searchCts.Token);
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
            _parishResults = new List<GdsOptionItem<string>>();
            _wardResults = new List<GdsOptionItem<string>>();
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

    private void ResetGeom()
    {
        Model.ByPolygon = null;
        StateHasChanged();
    }

}