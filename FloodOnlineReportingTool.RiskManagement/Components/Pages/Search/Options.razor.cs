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
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    ICommonRepository commonRepository,
    IGeoServerClient geoServerClient,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage,
    IConfiguration configuration
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
    private string _searchTerm = string.Empty;
    private bool _isSearching = false;
    private IReadOnlyCollection<GdsOptionItem<string>> _parishResults = new List<GdsOptionItem<string>>();
    private IReadOnlyCollection<GdsOptionItem<string>> _wardResults = new List<GdsOptionItem<string>>();
    private CancellationTokenSource? _searchCts;
    private string _searchArea = string.Empty;
    private string _mapURL = string.Empty;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _searchArea = configuration["RiskManagement:DefaultMapBoundingBox"] ?? "365955,77709,375836,92362";
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
            GenerateMapURL();

            _isLoading = false;
            StateHasChanged();
        }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        _floodSourceOptions = await CreateFloodSourceOptions(Model.FloodProblemIds);
        StateHasChanged();
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Options));
        }
    }

    private async Task OnSubmit()
    {
        //Store the search options in session storage for retrieval on the next page
        try
        {
            await protectedSessionStorage.SetAsync(SessionConstants.SearchOptions, Model);
        } catch (Exception ex)
        {
            logger.LogError(ex, "Issue setting protected storage for search options page");
        }

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

    private async Task OnSearch()
    {      
        // Clear results if search term is too short
        if (string.IsNullOrWhiteSpace(_searchTerm) || _searchTerm.Length < 3)
        {
            _parishResults = new List<GdsOptionItem<string>>();
            _wardResults = new List<GdsOptionItem<string>>();
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
            
            _parishResults = await geoServerClient.SearchNameAsync("parish", searchTerm, _searchCts.Token);
            _wardResults = await geoServerClient.SearchNameAsync("ward", searchTerm, _searchCts.Token);

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

            // Store selection in your model
            var geometry = await geoServerClient.GetGeometryFromSourceBySearch("parish", selectedParish, _cts.Token);

            if (geometry != null)
            {
                if (geometry is MultiPolygon multiPolygon)
                {
                    if (Model.ByAreaGeom != null)
                    {
                        var unionGeom = Model.ByAreaGeom.Union(geometry);
                        if (unionGeom is MultiPolygon newMultiPolygon)
                        {
                            Model.ByAreaGeom = newMultiPolygon;

                        } else if (unionGeom is Polygon singlePolygon)
                        {
                            Model.ByAreaGeom = new MultiPolygon(new[] { singlePolygon });
                        } else
                        {
                            logger.LogWarning("Union result is unexpected geometry type: {GeometryType}", unionGeom.GeometryType);
                        }
                    }
                    else
                    {
                        Model.ByAreaGeom = multiPolygon;
                    }
                } else
                {
                    logger.LogWarning("Geometry for parish {Parish} is not a MultiPolygon as expected. Type: {GeometryType}",
                        selectedParish, geometry.GeometryType);
                }
            }
            if (Model.ByAreaGeom != null)
            {
                Envelope envelope = Model.ByAreaGeom.EnvelopeInternal;
                double bufferDistance = 100.0;
                envelope.ExpandBy(bufferDistance);
                _searchArea = $"{envelope.MinX},{envelope.MinY},{envelope.MaxX},{envelope.MaxY}";
                GenerateMapURL();
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

            // Store selection in your model
            var geometry = await geoServerClient.GetGeometryFromSourceBySearch("ward", selectedWard, _cts.Token);

            if (geometry != null)
            {
                if (geometry is MultiPolygon multiPolygon)
                {
                    if (Model.ByAreaGeom != null)
                    {
                        var unionGeom = Model.ByAreaGeom.Union(geometry);
                        if (unionGeom is MultiPolygon newMultiPolygon)
                        {
                            Model.ByAreaGeom = newMultiPolygon;

                        }
                        else if (unionGeom is Polygon singlePolygon)
                        {
                            Model.ByAreaGeom = new MultiPolygon(new[] { singlePolygon });
                        }
                        else
                        {
                            logger.LogWarning("Union result is unexpected geometry type: {GeometryType}", unionGeom.GeometryType);
                        }
                    }
                    else
                    {
                        Model.ByAreaGeom = multiPolygon;
                    }
                }
                else
                {
                    logger.LogWarning("Geometry for ward {Ward} is not a MultiPolygon as expected. Type: {GeometryType}",
                        selectedWard, geometry.GeometryType);
                }
            }
            if (Model.ByAreaGeom != null)
            {
                Envelope envelope = Model.ByAreaGeom.EnvelopeInternal;
                double bufferDistance = 100.0;
                envelope.ExpandBy(bufferDistance);
                _searchArea = $"{envelope.MinX},{envelope.MinY},{envelope.MaxX},{envelope.MaxY}";
                GenerateMapURL();
            }
            StateHasChanged();
        }
    }

    private void ResetGeom()
    {
        Model.ByAreaGeom = null;
        StateHasChanged();
    }

    private string GetSvgViewBox()
    {
        if (Model.ByAreaGeom == null) return "0 0 500 300";

        var envelope = Model.ByAreaGeom.EnvelopeInternal;
        double bufferDistance = 100.0;
        envelope.ExpandBy(bufferDistance);

        return FormattableString.Invariant($"0 0 {envelope.Width:F2} {envelope.Height:F2}");
    }

    private string GenerateSvgPoints()
    {
        if (Model.ByAreaGeom == null) return string.Empty;

        var envelope = Model.ByAreaGeom.EnvelopeInternal;
        double bufferDistance = 100.0;
        envelope.ExpandBy(bufferDistance);

        var coordinates = Model.ByAreaGeom.Coordinates;
        var points = new StringBuilder();

        foreach (var coord in coordinates)
        {
            double x = coord.X - envelope.MinX;
            double y = envelope.MaxY - coord.Y; // Flip Y

            if (points.Length > 0) points.Append(' ');
            points.Append(FormattableString.Invariant($"{x:F2},{y:F2}"));
        }

        return points.ToString();
    }

    private void GenerateMapURL()
    {
        string MiniMapBaseUrl = configuration["RiskManagement:MiniMapBaseUrl"] ?? "";
        _mapURL = $"{MiniMapBaseUrl}&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&srs=epsg%3a27700&format=image%2fjpeg&width=500&height=300&bbox={_searchArea}";
    }
}