import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import proj4 from 'proj4';

interface WmsLayerConfig {
    id: string;
    url: string;
    layers: string;
    format: string;
}

const mapInstances = new Map<string, maplibregl.Map>();

// EPSG:27700 – British National Grid
const BNG_PROJ4 = '+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +ellps=airy +towgs84=446.448,-125.157,542.06,0.15,0.247,0.842,-20.489 +units=m +no_defs';
const WGS84_PROJ4 = '+proj=longlat +datum=WGS84 +no_defs';

const GEOMETRY_SOURCE_ID = 'mini-map-geometry';
const GEOMETRY_FILL_LAYER_ID = 'mini-map-geometry-fill';
const GEOMETRY_LINE_LAYER_ID = 'mini-map-geometry-line';
const GEOMETRY_CIRCLE_LAYER_ID = 'mini-map-geometry-circle';

const MARKERS_SOURCE_ID = 'mini-map-markers';
const MARKERS_LAYER_ID = 'mini-map-markers-circle';

interface MarkerData {
    id: string;
    latitude: number;
    longitude: number;
    popupHtml: string;
}

interface DotNetObject {
    invokeMethodAsync(methodName: string): Promise<void>;
}

const activePopups = new Map<string, maplibregl.Popup>();
const markerEventsRegistered = new Set<string>();

/**
 * Initialises a MapLibre GL map inside the element with the given ID, using the OS
 * Vector Tile Service as its basemap.
 *
 * @param containerId  The id of the HTML element to render the map into.
 * @param apiKey       OS Data Hub API key for the Vector Tile Service.
 * @param attribution  Attribution string added to the compact attribution control.
 * @param defaultBounds  Initial viewport bounds [minEasting, minNorthing, maxEasting, maxNorthing] in British National Grid (EPSG:27700).
 * @param wmsLayers    Optional WMS overlay layers to add once the map has loaded.
 */
export function initMiniMap(
    containerId: string,
    apiKey: string,
    attribution: string,
    defaultBounds: [number, number, number, number],
    wmsLayers: WmsLayerConfig[]
): void {
    const styleUrl = `https://api.os.uk/maps/vector/v1/vts/resources/styles?key=${encodeURIComponent(apiKey)}&srs=3857`;

    const wgs84Bounds = bngBoundsToWgs84(defaultBounds);

    const map = new maplibregl.Map({
        container: containerId,
        style: styleUrl,
        bounds: wgs84Bounds,
        fitBoundsOptions: { padding: 20 },
        attributionControl: false,
        maxPitch: 0,
        dragRotate: false,
    });

    map.addControl(
        new maplibregl.AttributionControl({ customAttribution: attribution, compact: true })
    );

    map.addControl(new maplibregl.NavigationControl({ showCompass: false }), 'top-right');

    if (wmsLayers.length > 0) {
        map.on('load', () => {
            wmsLayers.forEach(layer => {
                map.addSource(layer.id, {
                    type: 'raster',
                    tiles: [buildWmsTileUrl(layer)],
                    tileSize: 256,
                });
                map.addLayer({
                    id: layer.id,
                    type: 'raster',
                    source: layer.id,
                });
            });
        });
    }

    mapInstances.set(containerId, map);
}

/**
 * Updates (or clears) the geometry displayed on the map.
 *
 * @param containerId   The id of the map container element.
 * @param geoJson       A GeoJSON string (Geometry, Feature, or FeatureCollection) in British National Grid (EPSG:27700),
 *                      or null to remove any existing geometry.
 * @param zoomToBounds  When true the viewport will be fitted to the geometry's bounding box.
 */
export function updateGeometry(
    containerId: string,
    geoJson: string | null,
    zoomToBounds: boolean
): void {
    const map = mapInstances.get(containerId);
    if (!map) {
        return;
    }

    if (map.loaded()) {
        applyGeometry(map, geoJson, zoomToBounds);
    } else {
        map.once('load', () => applyGeometry(map, geoJson, zoomToBounds));
    }
}

/**
 * Updates (or clears) the markers displayed on the map.
 *
 * @param containerId   The id of the map container element.
 * @param markersJson   A JSON array of MarkerData objects, or null to remove any existing markers.
 */
export function updateMarkers(
    containerId: string,
    markersJson: string | null
): void {
    const map = mapInstances.get(containerId);
    if (!map) {
        return;
    }

    if (map.loaded()) {
        applyMarkers(map, containerId, markersJson);
    } else {
        map.once('load', () => applyMarkers(map, containerId, markersJson));
    }
}

/**
 * Observes the map container for visibility changes (e.g. when shown via GDS tabs)
 * and invokes a .NET callback the first time it becomes visible.
 *
 * @param containerId    The id of the map container element.
 * @param dotNetRef      A DotNetObjectReference to invoke the callback on.
 * @param callbackMethod The name of the [JSInvokable] method to call.
 */
export function observeVisibility(
    containerId: string,
    dotNetRef: DotNetObject,
    callbackMethod: string
): void {
    const container = document.getElementById(containerId);
    if (!container) {
        return;
    }

    const tabPanel = container.closest('.govuk-tabs__panel');
    if (!tabPanel) {
        // Not inside a GDS tabs panel – invoke immediately
        dotNetRef.invokeMethodAsync(callbackMethod);
        return;
    }

    // If the panel is already visible, invoke immediately
    if (!tabPanel.classList.contains('govuk-tabs__panel--hidden')) {
        dotNetRef.invokeMethodAsync(callbackMethod);
        return;
    }

    // Watch for the hidden class being removed
    const observer = new MutationObserver(() => {
        if (!tabPanel.classList.contains('govuk-tabs__panel--hidden')) {
            dotNetRef.invokeMethodAsync(callbackMethod);
            observer.disconnect();
        }
    });

    observer.observe(tabPanel, { attributes: true, attributeFilter: ['class'] });
}

/**
 * Removes the map instance and frees its resources.
 *
 * @param containerId  The id of the map container element.
 */
export function destroyMap(containerId: string): void {
    const map = mapInstances.get(containerId);
    if (map) {
        map.remove();
        mapInstances.delete(containerId);
    }
    activePopups.delete(containerId);
    markerEventsRegistered.delete(containerId);
}

// ---------------------------------------------------------------------------
// Private helpers
// ---------------------------------------------------------------------------

function bngToWgs84(easting: number, northing: number): [number, number] {
    return proj4(BNG_PROJ4, WGS84_PROJ4, [easting, northing]) as [number, number];
}

function bngBoundsToWgs84(bounds: [number, number, number, number]): [number, number, number, number] {
    const [minE, minN, maxE, maxN] = bounds;
    const corners = [
        bngToWgs84(minE, minN),
        bngToWgs84(maxE, minN),
        bngToWgs84(maxE, maxN),
        bngToWgs84(minE, maxN),
    ];
    const lngs = corners.map(c => c[0]);
    const lats = corners.map(c => c[1]);
    return [Math.min(...lngs), Math.min(...lats), Math.max(...lngs), Math.max(...lats)];
}

function reprojectCoordinates(coords: unknown): unknown {
    if (!Array.isArray(coords)) {
        return coords;
    }
    if (typeof coords[0] === 'number' && typeof coords[1] === 'number') {
        const [lng, lat] = bngToWgs84(coords[0], coords[1]);
        return coords.length > 2 ? [lng, lat, ...coords.slice(2)] : [lng, lat];
    }
    return (coords as unknown[]).map(reprojectCoordinates);
}

function reprojectGeometry(geometry: Record<string, unknown>): Record<string, unknown> {
    if (geometry.type === 'GeometryCollection') {
        return {
            ...geometry,
            geometries: (geometry.geometries as unknown[]).map(
                g => reprojectGeometry(g as Record<string, unknown>)
            ),
        };
    }
    return { ...geometry, coordinates: reprojectCoordinates(geometry.coordinates) };
}

function reprojectGeoJsonToWgs84(
    data: maplibregl.GeoJSONSourceSpecification['data']
): maplibregl.GeoJSONSourceSpecification['data'] {
    if (typeof data === 'string' || data === undefined) {
        return data;
    }
    const obj = data as unknown as Record<string, unknown>;
    if (obj.type === 'FeatureCollection') {
        return {
            ...obj,
            features: (obj.features as unknown[]).map(f => {
                const feature = f as Record<string, unknown>;
                return {
                    ...feature,
                    geometry: feature.geometry
                        ? reprojectGeometry(feature.geometry as Record<string, unknown>)
                        : null,
                };
            }),
        } as unknown as maplibregl.GeoJSONSourceSpecification['data'];
    }
    if (obj.type === 'Feature') {
        return {
            ...obj,
            geometry: obj.geometry
                ? reprojectGeometry(obj.geometry as Record<string, unknown>)
                : null,
        } as unknown as maplibregl.GeoJSONSourceSpecification['data'];
    }
    return reprojectGeometry(obj) as unknown as maplibregl.GeoJSONSourceSpecification['data'];
}

function applyGeometry(
    map: maplibregl.Map,
    geoJson: string | null,
    zoomToBounds: boolean
): void {
    removeGeometryLayers(map);

    if (!geoJson) {
        return;
    }

    let data: maplibregl.GeoJSONSourceSpecification['data'];
    try {
        data = JSON.parse(geoJson) as maplibregl.GeoJSONSourceSpecification['data'];
    } catch {
        console.warn('MiniMap: invalid GeoJSON supplied – geometry will not be rendered.');
        return;
    }

    const wgs84Data = reprojectGeoJsonToWgs84(data);

    map.addSource(GEOMETRY_SOURCE_ID, { type: 'geojson', data: wgs84Data });

    // Fill layer for polygon geometry
    map.addLayer({
        id: GEOMETRY_FILL_LAYER_ID,
        type: 'fill',
        source: GEOMETRY_SOURCE_ID,
        filter: ['match', ['geometry-type'], ['Polygon', 'MultiPolygon'], true, false],
        paint: {
            'fill-color': '#1d70b8',
            'fill-opacity': 0.3,
        },
    });

    // Line layer for polygon outlines and linestrings
    map.addLayer({
        id: GEOMETRY_LINE_LAYER_ID,
        type: 'line',
        source: GEOMETRY_SOURCE_ID,
        filter: [
            'match',
            ['geometry-type'],
            ['Polygon', 'MultiPolygon', 'LineString', 'MultiLineString'],
            true,
            false,
        ],
        paint: {
            'line-color': '#1d70b8',
            'line-width': 2,
        },
    });

    // Circle layer for point geometry
    map.addLayer({
        id: GEOMETRY_CIRCLE_LAYER_ID,
        type: 'circle',
        source: GEOMETRY_SOURCE_ID,
        filter: ['match', ['geometry-type'], ['Point', 'MultiPoint'], true, false],
        paint: {
            'circle-color': '#1d70b8',
            'circle-radius': 6,
            'circle-stroke-color': '#ffffff',
            'circle-stroke-width': 2,
        },
    });

    if (zoomToBounds) {
        fitToGeometry(map, wgs84Data);
    }
}

function removeGeometryLayers(map: maplibregl.Map): void {
    for (const layerId of [GEOMETRY_FILL_LAYER_ID, GEOMETRY_LINE_LAYER_ID, GEOMETRY_CIRCLE_LAYER_ID]) {
        if (map.getLayer(layerId)) {
            map.removeLayer(layerId);
        }
    }
    if (map.getSource(GEOMETRY_SOURCE_ID)) {
        map.removeSource(GEOMETRY_SOURCE_ID);
    }
}

type Coordinate = number[];

function fitToGeometry(
    map: maplibregl.Map,
    data: maplibregl.GeoJSONSourceSpecification['data']
): void {
    if (typeof data === 'string') {
        return;
    }

    const bounds = calculateBounds(data);
    if (!bounds) {
        return;
    }

    const [west, south, east, north] = bounds;

    // For near-point geometries, fly to a fixed zoom rather than fitBounds
    if (Math.abs(east - west) < 0.0001 && Math.abs(north - south) < 0.0001) {
        map.flyTo({ center: [(west + east) / 2, (south + north) / 2], zoom: 14 });
    } else {
        map.fitBounds([[west, south], [east, north]], { padding: 50, maxZoom: 16 });
    }
}

function calculateBounds(
    data: Exclude<maplibregl.GeoJSONSourceSpecification['data'], string | undefined>
): [number, number, number, number] | null {
    let minX = Infinity;
    let minY = Infinity;
    let maxX = -Infinity;
    let maxY = -Infinity;

    function visitCoord(coord: Coordinate): void {
        const x = coord[0];
        const y = coord[1];
        if (x !== undefined && y !== undefined) {
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }
    }

    function visitCoords(coords: unknown): void {
        if (!Array.isArray(coords)) {
            return;
        }
        if (typeof coords[0] === 'number') {
            visitCoord(coords as Coordinate);
        } else {
            for (const child of coords) {
                visitCoords(child);
            }
        }
    }

    function visitGeometry(geom: { type: string; coordinates?: unknown; geometries?: unknown[] }): void {
        if (geom.type === 'GeometryCollection' && Array.isArray(geom.geometries)) {
            for (const child of geom.geometries) {
                visitGeometry(child as { type: string; coordinates?: unknown; geometries?: unknown[] });
            }
        } else {
            visitCoords(geom.coordinates);
        }
    }

    const obj = data as { type: string; features?: unknown[]; geometry?: unknown; coordinates?: unknown; geometries?: unknown[] };

    if (obj.type === 'FeatureCollection' && Array.isArray(obj.features)) {
        for (const f of obj.features) {
            const feature = f as { geometry?: { type: string; coordinates?: unknown; geometries?: unknown[] } };
            if (feature.geometry) {
                visitGeometry(feature.geometry);
            }
        }
    } else if (obj.type === 'Feature') {
        const feature = obj as { geometry?: { type: string; coordinates?: unknown; geometries?: unknown[] } };
        if (feature.geometry) {
            visitGeometry(feature.geometry);
        }
    } else {
        visitGeometry(obj as { type: string; coordinates?: unknown; geometries?: unknown[] });
    }

    if (!isFinite(minX) || !isFinite(minY) || !isFinite(maxX) || !isFinite(maxY)) {
        return null;
    }

    return [minX, minY, maxX, maxY];
}

function applyMarkers(
    map: maplibregl.Map,
    containerId: string,
    markersJson: string | null
): void {
    removeMarkerLayers(map, containerId);

    if (!markersJson) {
        return;
    }

    let markers: MarkerData[];
    try {
        markers = JSON.parse(markersJson) as MarkerData[];
    } catch {
        console.warn('MiniMap: invalid markers JSON supplied – markers will not be rendered.');
        return;
    }

    if (markers.length === 0) {
        return;
    }

    const features = markers.map(m => ({
        type: 'Feature' as const,
        geometry: {
            type: 'Point' as const,
            coordinates: [m.longitude, m.latitude] as [number, number]
        },
        properties: {
            id: m.id,
            popupHtml: m.popupHtml
        }
    }));

    map.addSource(MARKERS_SOURCE_ID, {
        type: 'geojson',
        data: {
            type: 'FeatureCollection' as const,
            features
        } as maplibregl.GeoJSONSourceSpecification['data']
    });

    map.addLayer({
        id: MARKERS_LAYER_ID,
        type: 'circle',
        source: MARKERS_SOURCE_ID,
        paint: {
            'circle-color': '#d4351c',
            'circle-radius': 7,
            'circle-stroke-color': '#ffffff',
            'circle-stroke-width': 2,
        }
    });

    if (!markerEventsRegistered.has(containerId)) {
        map.on('mouseenter', MARKERS_LAYER_ID, () => {
            map.getCanvas().style.cursor = 'pointer';
        });

        map.on('mouseleave', MARKERS_LAYER_ID, () => {
            map.getCanvas().style.cursor = '';
        });

        map.on('click', MARKERS_LAYER_ID, (e: maplibregl.MapLayerMouseEvent) => {
            if (!e.features || e.features.length === 0) return;

            const feature = e.features[0];
            const geom = feature.geometry as { type: string; coordinates: number[] };
            const coords = geom.coordinates.slice() as [number, number];
            const popupHtml = (feature.properties?.popupHtml as string) ?? '';

            const existing = activePopups.get(containerId);
            if (existing) existing.remove();

            const popup = new maplibregl.Popup({ offset: 10, maxWidth: '300px' })
                .setLngLat(coords)
                .setHTML(popupHtml)
                .addTo(map);

            activePopups.set(containerId, popup);
        });

        markerEventsRegistered.add(containerId);
    }

    fitToMarkerBounds(map, markers);
}

function removeMarkerLayers(map: maplibregl.Map, containerId: string): void {
    if (map.getLayer(MARKERS_LAYER_ID)) {
        map.removeLayer(MARKERS_LAYER_ID);
    }
    if (map.getSource(MARKERS_SOURCE_ID)) {
        map.removeSource(MARKERS_SOURCE_ID);
    }
    const popup = activePopups.get(containerId);
    if (popup) {
        popup.remove();
        activePopups.delete(containerId);
    }
}

function fitToMarkerBounds(map: maplibregl.Map, markers: MarkerData[]): void {
    if (markers.length === 0) return;

    let minLng = Infinity, minLat = Infinity, maxLng = -Infinity, maxLat = -Infinity;
    for (const m of markers) {
        if (m.longitude < minLng) minLng = m.longitude;
        if (m.latitude < minLat) minLat = m.latitude;
        if (m.longitude > maxLng) maxLng = m.longitude;
        if (m.latitude > maxLat) maxLat = m.latitude;
    }

    if (Math.abs(maxLng - minLng) < 0.0001 && Math.abs(maxLat - minLat) < 0.0001) {
        map.flyTo({ center: [(minLng + maxLng) / 2, (minLat + maxLat) / 2], zoom: 14 });
    } else {
        map.fitBounds([[minLng, minLat], [maxLng, maxLat]], { padding: 50, maxZoom: 16 });
    }
}

function buildWmsTileUrl(layer: WmsLayerConfig): string {
    // Uses WMS 1.1.1 with the 'SRS' parameter (WMS 1.3.0 uses 'CRS' instead).
    // The FORMAT and LAYERS values are URL-encoded to safely handle special characters.
    const separator = layer.url.includes('?') ? '&' : '?';
    return (
        `${layer.url}${separator}` +
        `SERVICE=WMS&REQUEST=GetMap` +
        `&LAYERS=${encodeURIComponent(layer.layers)}` +
        `&STYLES=` +
        `&FORMAT=${encodeURIComponent(layer.format)}` +
        `&TRANSPARENT=TRUE` +
        `&VERSION=1.1.1` +
        `&SRS=EPSG:3857` +
        `&BBOX={bbox-epsg-3857}` +
        `&WIDTH=256&HEIGHT=256`
    );
}
