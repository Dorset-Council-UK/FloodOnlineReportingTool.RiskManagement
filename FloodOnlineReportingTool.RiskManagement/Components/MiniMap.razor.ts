import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';

interface WmsLayerConfig {
    id: string;
    url: string;
    layers: string;
    format: string;
}

const mapInstances = new Map<string, maplibregl.Map>();

const GEOMETRY_SOURCE_ID = 'mini-map-geometry';
const GEOMETRY_FILL_LAYER_ID = 'mini-map-geometry-fill';
const GEOMETRY_LINE_LAYER_ID = 'mini-map-geometry-line';
const GEOMETRY_CIRCLE_LAYER_ID = 'mini-map-geometry-circle';

/**
 * Initialises a MapLibre GL map inside the element with the given ID, using the OS
 * Vector Tile Service as its basemap.
 *
 * @param containerId  The id of the HTML element to render the map into.
 * @param apiKey       OS Data Hub API key for the Vector Tile Service.
 * @param attribution  Attribution string added to the compact attribution control.
 * @param defaultBounds  Initial viewport bounds [west, south, east, north] in WGS84.
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

    const map = new maplibregl.Map({
        container: containerId,
        style: styleUrl,
        bounds: defaultBounds,
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
 * @param geoJson       A GeoJSON string (Geometry, Feature, or FeatureCollection) in WGS84,
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
}

// ---------------------------------------------------------------------------
// Private helpers
// ---------------------------------------------------------------------------

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

    map.addSource(GEOMETRY_SOURCE_ID, { type: 'geojson', data });

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
        fitToGeometry(map, data);
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
