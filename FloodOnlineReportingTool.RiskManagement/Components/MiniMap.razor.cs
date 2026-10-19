using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Components;

/// <summary>
/// A compact, interactive map component that uses MapLibre GL JS with the
/// Ordnance Survey Vector Tile Service as its basemap. Supports WMS overlay
/// layers and rendering of GeoJSON or NetTopologySuite Geometry objects.
/// </summary>
public partial class MiniMap : IAsyncDisposable
{
    private static readonly JsonSerializerOptions GeoJsonSerializerOptions = new()
    {
        Converters = { new GeoJsonConverterFactory() }
    };

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IOptions<MiniMapOptions> MapOptions { get; set; } = default!;

    /// <summary>
    /// Overrides the default map bounds from configuration.
    /// Expects British National Grid [MinX, MinY, MaxX, MaxY].
    /// </summary>
    [Parameter]
    public double[]? DefaultBounds { get; set; }

    /// <summary>
    /// Optional WMS-based overlay layers to display on top of the basemap.
    /// </summary>
    [Parameter]
    public IReadOnlyList<WmsLayerConfig>? WmsLayers { get; set; }

    /// <summary>
    /// A GeoJSON string (Feature, FeatureCollection, or Geometry) in British National Grid to render on the map.
    /// Takes precedence over <see cref="Geometry"/> if both are supplied.
    /// </summary>
    [Parameter]
    public string? GeoJson { get; set; }

    /// <summary>
    /// A NetTopologySuite <see cref="NetTopologySuite.Geometries.Geometry"/> in British National Grid
    /// to render on the map. Ignored if <see cref="GeoJson"/> is also supplied.
    /// </summary>
    [Parameter]
    public Geometry? Geometry { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the map viewport will zoom to fit the supplied geometry.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ZoomToGeometry { get; set; } = true;

    /// <summary>
    /// Clusters nearby markers and geometry points. Defaults to <see langword="true"/>.
    /// Lines and polygons are not clustered.
    /// </summary>
    [Parameter]
    public bool UseClustering { get; set; } = true;

    /// <summary>
    /// Callback invoked the first time the map container becomes visible (e.g. when
    /// a GDS tabs panel is shown). Useful for deferring expensive data loads until the
    /// map is actually viewed.
    /// </summary>
    [Parameter]
    public EventCallback OnBecameVisible { get; set; }

    private readonly string _mapId = $"mini-map-{Guid.NewGuid():N}";
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MiniMap>? _dotNetRef;
    private bool _mapInitialized;
    private bool _lastUseClustering;
    private string? _lastParameterGeoJson;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var options = MapOptions.Value;
        var bounds = DefaultBounds ?? options.DefaultBounds;
        var attribution = BuildAttribution(options.OsLicenceNumber);

        var wmsLayerConfigs = WmsLayers?
            .Select(l => new { id = l.Id, url = l.Url, layers = l.Layers, format = l.Format })
            .ToArray() ?? [];

        var jsPath = NavigationManager.ToAbsoluteUri("js/components/minimap.js").ToString();
        _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", jsPath);

        await _jsModule.InvokeVoidAsync("initMiniMap",
            _mapId,
            options.OsApiKey,
            attribution,
            bounds,
            wmsLayerConfigs,
            UseClustering);

        _lastUseClustering = UseClustering;
        _mapInitialized = true;

        if (OnBecameVisible.HasDelegate)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("observeVisibility", _mapId, _dotNetRef, nameof(OnMapBecameVisible));
        }

        var geoJson = GetGeoJson();
        if (geoJson is not null)
        {
            _lastParameterGeoJson = geoJson;
            await _jsModule.InvokeVoidAsync("updateGeometry", _mapId, geoJson, ZoomToGeometry);
        }
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (!_mapInitialized || _jsModule is null)
        {
            return;
        }

        if (_lastUseClustering != UseClustering)
        {
            await _jsModule.InvokeVoidAsync("updateClustering", _mapId, UseClustering);
            _lastUseClustering = UseClustering;
        }

        var geoJson = GetGeoJson();
        if (string.Equals(geoJson, _lastParameterGeoJson, StringComparison.Ordinal))
        {
            return;
        }

        _lastParameterGeoJson = geoJson;
        await _jsModule.InvokeVoidAsync("updateGeometry", _mapId, geoJson, ZoomToGeometry);
    }

    /// <summary>
    /// Programmatically updates the geometry rendered on the map.
    /// </summary>
    /// <param name="geometry">
    /// The new geometry to display, in WGS84 (SRID 4326). Pass <see langword="null"/> to clear.
    /// </param>
    /// <param name="zoomToGeometry">
    /// Whether to zoom the viewport to fit the geometry. Defaults to <see langword="true"/>.
    /// </param>
    public async Task UpdateGeometryAsync(Geometry? geometry, bool zoomToGeometry = true)
    {
        if (_jsModule is null)
        {
            return;
        }

        var geoJson = geometry is not null ? SerializeGeometry(geometry) : null;
        await _jsModule.InvokeVoidAsync("updateGeometry", _mapId, geoJson, zoomToGeometry);
    }

    /// <summary>
    /// Programmatically updates the GeoJSON data rendered on the map.
    /// </summary>
    /// <param name="geoJson">
    /// A GeoJSON string in WGS84 to display. Pass <see langword="null"/> to clear.
    /// </param>
    /// <param name="zoomToGeometry">
    /// Whether to zoom the viewport to fit the geometry. Defaults to <see langword="true"/>.
    /// </param>
    public async Task UpdateGeoJsonAsync(string? geoJson, bool zoomToGeometry = true)
    {
        if (_jsModule is null)
        {
            return;
        }

        await _jsModule.InvokeVoidAsync("updateGeometry", _mapId, geoJson, zoomToGeometry);
    }

    /// <summary>
    /// Programmatically updates the markers displayed on the map.
    /// </summary>
    /// <param name="markers">
    /// The markers to display. Pass <see langword="null"/> or an empty list to clear.
    /// </param>
    public async Task UpdateMarkersAsync(IReadOnlyList<MapMarkerData>? markers)
    {
        if (_jsModule is null)
        {
            return;
        }

        string? json = null;
        if (markers is { Count: > 0 })
        {
            json = JsonSerializer.Serialize(markers, JsonSerializerOptions.Web);
        }

        await _jsModule.InvokeVoidAsync("updateMarkers", _mapId, json);
    }

    /// <summary>
    /// Called from JavaScript when the map container first becomes visible.
    /// </summary>
    [JSInvokable]
    public async Task OnMapBecameVisible()
    {
        await OnBecameVisible.InvokeAsync();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("destroyMap", _mapId);
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The Blazor circuit has disconnected; disposal via JS is not required.
            }
        }

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }

    private string? GetGeoJson()
    {
        if (GeoJson is not null)
        {
            return GeoJson;
        }

        if (Geometry is not null)
        {
            return SerializeGeometry(Geometry);
        }

        return null;
    }

    private static string SerializeGeometry(Geometry geometry) =>
        JsonSerializer.Serialize(geometry, GeoJsonSerializerOptions);

    private static string BuildAttribution(string licenceNumber)
    {
        var year = DateTime.UtcNow.Year;
        var attribution = $"© Crown copyright and database rights {year} OS";
        if (!string.IsNullOrWhiteSpace(licenceNumber))
        {
            attribution += $" {licenceNumber}";
        }

        return attribution;
    }
}
