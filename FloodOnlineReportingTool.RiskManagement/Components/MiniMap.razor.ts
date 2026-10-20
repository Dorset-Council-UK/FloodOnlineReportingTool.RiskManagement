import {AttributionControl, GeoJSONSource, GeoJSONSourceSpecification, MapLayerMouseEvent, Map as MapLibreMap, NavigationControl, Popup, setWorkerUrl} from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import proj4 from 'proj4';
import type { Feature, FeatureCollection, GeoJSON, Geometry, Point } from 'geojson';

setWorkerUrl(new URL('js/workers/maplibre-gl-worker.mjs', document.baseURI).toString());

interface WmsLayerConfig {
    id: string;
    url: string;
    layers: string;
    format: string;
}

const mapInstances = new Map<string, MapLibreMap>();
const styleReadyMaps = new WeakSet<MapLibreMap>();
const visibilityObservers = new Map<string, MutationObserver>();

interface MapDataState {
    useClustering: boolean;
    geoJson: string | null;
    markersJson: string | null;
    zoomToGeometry: boolean;
    geometryQueued: boolean;
    markersQueued: boolean;
    clusterClickVersion: number;
}

const mapDataStates = new WeakMap<MapLibreMap, MapDataState>();
const clusterEventsRegistered = new WeakMap<MapLibreMap, Set<string>>();

// EPSG:27700 – British National Grid
const BNG_PROJ4 = '+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +ellps=airy +towgs84=446.448,-125.157,542.06,0.15,0.247,0.842,-20.489 +units=m +no_defs';
const WGS84_PROJ4 = '+proj=longlat +datum=WGS84 +no_defs';

const GEOMETRY_SOURCE_ID = 'mini-map-geometry';
const GEOMETRY_FILL_LAYER_ID = 'mini-map-geometry-fill';
const GEOMETRY_LINE_LAYER_ID = 'mini-map-geometry-line';
const GEOMETRY_CIRCLE_LAYER_ID = 'mini-map-geometry-circle';
const GEOMETRY_POINTS_SOURCE_ID = 'mini-map-geometry-points';

const MARKERS_SOURCE_ID = 'mini-map-markers';
const MARKERS_LAYER_ID = 'mini-map-markers-circle';

const CLUSTER_OPTIONS = { cluster: true, clusterRadius: 50, clusterMaxZoom: 14, clusterMinPoints: 2 };

interface MarkerData {
    id: string;
    latitude: number;
    longitude: number;
    popupHtml: string;
}

const markerSelections = new WeakMap<MapLibreMap, { markers: MarkerData[]; groups: Map<string, MarkerData[]> }>();
const MARKER_INDEX = '__miniMapMarkerIndex';

function markerLocationKey(longitude: number, latitude: number): string {
    return JSON.stringify([longitude, latitude]);
}

interface DotNetObject {
    invokeMethodAsync(methodName: string): Promise<void>;
}

const activePopups = new Map<string, Popup>();
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
 * @param useClustering Whether nearby markers and geometry points should be clustered.
 */
export function initMiniMap(
    containerId: string,
    apiKey: string,
    attribution: string,
    defaultBounds: [number, number, number, number],
    wmsLayers: WmsLayerConfig[],
    useClustering = true
): void {
    const styleUrl = `https://api.os.uk/maps/vector/v1/vts/resources/styles?key=${encodeURIComponent(apiKey)}&srs=3857`;

    const wgs84Bounds = bngBoundsToWgs84(defaultBounds);

    const map = new MapLibreMap({
        container: containerId,
        style: styleUrl,
        bounds: wgs84Bounds,
        fitBoundsOptions: { padding: 20 },
        attributionControl: false,
        maxPitch: 0,
        dragRotate: false,
        transformRequest: (url) => {
            const resourceUrl = new URL(url, document.baseURI);
            if (resourceUrl.origin === 'https://api.os.uk') {
                resourceUrl.searchParams.set('key', apiKey);
                return { url: resourceUrl.toString() };
            }
            return { url };
        },
    });

    mapDataStates.set(map, {
        useClustering, geoJson: null, markersJson: null, zoomToGeometry: true,
        geometryQueued: false, markersQueued: false,
        clusterClickVersion: 0,
    });

    map.addControl(
        new AttributionControl({ customAttribution: attribution, compact: true })
    );

    map.addControl(new NavigationControl({ showCompass: false }), 'top-right');

    map.once('style.load', () => {
        if (mapInstances.get(containerId) !== map) return;
        styleReadyMaps.add(map);
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

    mapInstances.set(containerId, map);
    watchVisibility(containerId);
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

    const state = mapDataStates.get(map)!;
    state.geoJson = geoJson;
    state.zoomToGeometry = zoomToBounds;
    if (styleReadyMaps.has(map)) {
        applyGeometry(map, geoJson, zoomToBounds);
    } else if (!state.geometryQueued) {
        state.geometryQueued = true;
        map.once('style.load', () => {
            state.geometryQueued = false;
            if (mapInstances.get(containerId) === map) {
                applyGeometry(map, state.geoJson, state.zoomToGeometry);
            }
        });
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

    const state = mapDataStates.get(map)!;
    state.markersJson = markersJson;
    if (styleReadyMaps.has(map)) {
        applyMarkers(map, containerId, markersJson);
    } else if (!state.markersQueued) {
        state.markersQueued = true;
        map.once('style.load', () => {
            state.markersQueued = false;
            if (mapInstances.get(containerId) === map) {
                applyMarkers(map, containerId, state.markersJson);
            }
        });
    }
}

export function updateClustering(containerId: string, useClustering: boolean): void {
    const map = mapInstances.get(containerId);
    if (!map) return;

    const state = mapDataStates.get(map)!;
    if (state.useClustering === useClustering) return;
    state.useClustering = useClustering;
    if (styleReadyMaps.has(map)) {
        applyGeometry(map, state.geoJson, false);
        applyMarkers(map, containerId, state.markersJson, false);
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
    watchVisibility(containerId, () => {
        void dotNetRef.invokeMethodAsync(callbackMethod);
    });
}

function watchVisibility(containerId: string, onFirstVisible?: () => void): void {
    visibilityObservers.get(containerId)?.disconnect();
    visibilityObservers.delete(containerId);

    const container = document.getElementById(containerId);
    if (!container) {
        return;
    }

    const tabPanel = container.closest('.govuk-tabs__panel');
    let notified = false;
    const handleVisibility = () => {
        if (tabPanel?.classList.contains('govuk-tabs__panel--hidden')) {
            return;
        }

        mapInstances.get(containerId)?.resize();
        if (!notified) {
            notified = true;
            onFirstVisible?.();
        }
    };

    handleVisibility();
    if (!tabPanel) {
        return;
    }

    const observer = new MutationObserver(handleVisibility);
    observer.observe(tabPanel, { attributes: true, attributeFilter: ['class'] });
    visibilityObservers.set(containerId, observer);
}

/**
 * Removes the map instance and frees its resources.
 *
 * @param containerId  The id of the map container element.
 */
export function destroyMap(containerId: string): void {
    visibilityObservers.get(containerId)?.disconnect();
    visibilityObservers.delete(containerId);
    activePopups.get(containerId)?.remove();
    const map = mapInstances.get(containerId);
    if (map) {
        map.remove();
        mapInstances.delete(containerId);
        mapDataStates.delete(map);
        markerSelections.delete(map);
        styleReadyMaps.delete(map);
        clusterEventsRegistered.delete(map);
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
    data: GeoJSONSourceSpecification['data']
): GeoJSONSourceSpecification['data'] {
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
        } as unknown as GeoJSONSourceSpecification['data'];
    }
    if (obj.type === 'Feature') {
        return {
            ...obj,
            geometry: obj.geometry
                ? reprojectGeometry(obj.geometry as Record<string, unknown>)
                : null,
        } as unknown as GeoJSONSourceSpecification['data'];
    }
    return reprojectGeometry(obj) as unknown as GeoJSONSourceSpecification['data'];
}

function splitGeometryPoints(data: GeoJSON): { points: FeatureCollection<Point>; other: FeatureCollection } {
    const points: FeatureCollection<Point> = { type: 'FeatureCollection', features: [] };
    const other: FeatureCollection = { type: 'FeatureCollection', features: [] };
    const features: Feature[] = data.type === 'FeatureCollection' ? data.features
        : data.type === 'Feature' ? [data]
        : [{ type: 'Feature', properties: {}, geometry: data }];

    for (const feature of features) {
        function extract(geometry: Geometry, path: number[] = []): Geometry | null {
            if (geometry.type === 'Point') {
                const point: Feature<Point> = { ...feature, geometry };
                if (feature.id !== undefined && path.length > 0) {
                    point.id = JSON.stringify([feature.id, path]);
                }
                points.features.push(point);
                return null;
            }
            if (geometry.type === 'MultiPoint') {
                geometry.coordinates.forEach((coordinates, index) => {
                    extract({ type: 'Point', coordinates }, [...path, index]);
                });
                return null;
            }
            if (geometry.type === 'GeometryCollection') {
                const geometries = geometry.geometries
                    .map((child, index) => extract(child, [...path, index]))
                    .filter((child): child is Geometry => child !== null);
                return geometries.length > 0 ? { ...geometry, geometries } : null;
            }
            return geometry;
        }

        const geometry = feature.geometry ? extract(feature.geometry) : null;
        if (geometry) {
            other.features.push({ ...feature, geometry });
        }
    }
    return { points, other };
}

function applyGeometry(
    map: MapLibreMap,
    geoJson: string | null,
    zoomToBounds: boolean
): void {
    removeGeometryLayers(map);

    if (!geoJson) {
        return;
    }

    let data: GeoJSONSourceSpecification['data'];
    try {
        data = JSON.parse(geoJson) as GeoJSONSourceSpecification['data'];
    } catch {
        console.warn('MiniMap: invalid GeoJSON supplied – geometry will not be rendered.');
        return;
    }

    const wgs84Data = reprojectGeoJsonToWgs84(data);
    const useClustering = mapDataStates.get(map)!.useClustering;
    const split = useClustering ? splitGeometryPoints(wgs84Data as GeoJSON) : null;

    map.addSource(GEOMETRY_SOURCE_ID, { type: 'geojson', data: split?.other ?? wgs84Data });
    if (split) {
        map.addSource(GEOMETRY_POINTS_SOURCE_ID, { type: 'geojson', data: split.points, ...CLUSTER_OPTIONS });
        addClusterLayers(map, GEOMETRY_POINTS_SOURCE_ID, '#1d70b8');
    }

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
        source: split ? GEOMETRY_POINTS_SOURCE_ID : GEOMETRY_SOURCE_ID,
        filter: split ? ['!', ['has', 'point_count']]
            : ['match', ['geometry-type'], ['Point', 'MultiPoint'], true, false],
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

function removeGeometryLayers(map: MapLibreMap): void {
    removeClusterLayers(map, GEOMETRY_POINTS_SOURCE_ID);
    for (const layerId of [GEOMETRY_FILL_LAYER_ID, GEOMETRY_LINE_LAYER_ID, GEOMETRY_CIRCLE_LAYER_ID]) {
        if (map.getLayer(layerId)) {
            map.removeLayer(layerId);
        }
    }
    for (const sourceId of [GEOMETRY_SOURCE_ID, GEOMETRY_POINTS_SOURCE_ID]) {
        if (map.getSource(sourceId)) {
            map.removeSource(sourceId);
        }
    }
}

type Coordinate = number[];

function fitToGeometry(
    map: MapLibreMap,
    data: GeoJSONSourceSpecification['data']
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
    data: Exclude<GeoJSONSourceSpecification['data'], string | undefined>
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

function addClusterLayers(map: MapLibreMap, sourceId: string, color: string): void {
    const textFont = map.getStyle().layers?.flatMap(layer => {
        if (layer.type !== 'symbol') return [];
        const font = layer.layout?.['text-font'];
        return Array.isArray(font) && font.length > 0 && font.every(value => typeof value === 'string')
            ? [font as string[]] : [];
    })[0];

    map.addLayer({
        id: `${sourceId}-clusters`,
        type: 'circle',
        source: sourceId,
        filter: ['has', 'point_count'],
        paint: {
            'circle-color': color,
            'circle-radius': ['step', ['get', 'point_count'], 18, 10, 22, 100, 28],
            'circle-stroke-color': '#ffffff',
            'circle-stroke-width': 2,
        },
    });
    map.addLayer({
        id: `${sourceId}-cluster-count`,
        type: 'symbol',
        source: sourceId,
        filter: ['has', 'point_count'],
        layout: {
            'text-field': ['get', 'point_count_abbreviated'],
            'text-size': 12,
            'text-allow-overlap': true,
            ...(textFont ? { 'text-font': textFont } : {}),
        },
        paint: { 'text-color': '#ffffff' },
    });
    registerClusterEvents(map, sourceId);
}

function registerClusterEvents(map: MapLibreMap, sourceId: string): void {
    const registered = clusterEventsRegistered.get(map) ?? new Set<string>();
    if (registered.has(sourceId)) return;
    registered.add(sourceId);
    clusterEventsRegistered.set(map, registered);

    const layerId = `${sourceId}-clusters`;
    map.on('mouseenter', layerId, () => {
        map.getCanvas().style.cursor = 'pointer';
    });
    map.on('mouseleave', layerId, () => {
        map.getCanvas().style.cursor = '';
    });
    map.on('click', layerId, event => {
        void expandCluster(map, sourceId, event);
    });
}

async function expandCluster(map: MapLibreMap, sourceId: string, event: MapLayerMouseEvent): Promise<void> {
    const feature = event.features?.[0];
    const clusterId: unknown = feature?.properties?.cluster_id;
    const source = map.getSource<GeoJSONSource>(sourceId);
    const state = mapDataStates.get(map);
    if (!state || !source || feature?.geometry.type !== 'Point' || typeof clusterId !== 'number') return;

    const center = feature.geometry.coordinates.slice(0, 2) as [number, number];
    const version = ++state.clusterClickVersion;
    try {
        const zoom = await source.getClusterExpansionZoom(clusterId);
        if (mapDataStates.get(map) !== state || map.getSource(sourceId) !== source
            || state.clusterClickVersion !== version) return;
        map.easeTo({ center, zoom });
    } catch (error) {
        if (mapDataStates.get(map) === state && map.getSource(sourceId) === source) {
            console.warn('MiniMap: unable to expand cluster.', error);
        }
    }
}

function removeClusterLayers(map: MapLibreMap, sourceId: string): void {
    for (const layerId of [`${sourceId}-cluster-count`, `${sourceId}-clusters`]) {
        if (map.getLayer(layerId)) map.removeLayer(layerId);
    }
    map.getCanvas().style.cursor = '';
}

function applyMarkers(
    map: MapLibreMap,
    containerId: string,
    markersJson: string | null,
    zoomToBounds = true
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

    const groups = new Map<string, MarkerData[]>();
    for (const marker of markers) {
        const key = markerLocationKey(marker.longitude, marker.latitude);
        const group = groups.get(key) ?? [];
        group.push(marker);
        groups.set(key, group);
    }
    markerSelections.set(map, { markers, groups });

    const features = markers.map((m, index) => ({
        type: 'Feature' as const,
        geometry: {
            type: 'Point' as const,
            coordinates: [m.longitude, m.latitude] as [number, number]
        },
        properties: {
            [MARKER_INDEX]: index,
            id: m.id,
            popupHtml: m.popupHtml
        }
    }));

    const useClustering = mapDataStates.get(map)!.useClustering;
    map.addSource(MARKERS_SOURCE_ID, {
        type: 'geojson',
        ...(useClustering ? CLUSTER_OPTIONS : {}),
        data: {
            type: 'FeatureCollection' as const,
            features
        } as GeoJSONSourceSpecification['data']
    });

    if (useClustering) {
        addClusterLayers(map, MARKERS_SOURCE_ID, '#d4351c');
    }

    map.addLayer({
        id: MARKERS_LAYER_ID,
        type: 'circle',
        source: MARKERS_SOURCE_ID,
        ...(useClustering ? { filter: ['!', ['has', 'point_count']] as const } : {}),
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

        map.on('click', MARKERS_LAYER_ID, (e: MapLayerMouseEvent) => {
            if (!e.features || e.features.length === 0) return;

            const feature = e.features[0];
            const selection = markerSelections.get(map);
            const index = feature.properties?.[MARKER_INDEX];
            const marker = typeof index === 'number' ? selection?.markers[index] : undefined;
            if (!marker) return;
            const group = selection!.groups.get(markerLocationKey(marker.longitude, marker.latitude))!;
            const coords: [number, number] = [group[0]!.longitude, group[0]!.latitude];

            const existing = activePopups.get(containerId);
            if (existing) existing.remove();

            const popup = new Popup({ offset: 10, maxWidth: '300px' })
                .setLngLat(coords);
            if (group.length === 1) {
                popup.setHTML(group[0]!.popupHtml);
            } else {
                addMarkerNavigation(popup, group);
            }
            popup.addTo(map);

            activePopups.set(containerId, popup);
        });

        markerEventsRegistered.add(containerId);
    }

    if (zoomToBounds) {
        fitToMarkerBounds(map, markers);
    }
}

function addMarkerNavigation(popup: Popup, markers: MarkerData[]): void {
    const root = document.createElement('div');
    const content = document.createElement('div');
    const navigation = document.createElement('div');
    navigation.setAttribute('role', 'group');
    navigation.setAttribute('aria-label', 'Markers at this location');
    const previous = document.createElement('button');
    previous.type = 'button';
    previous.textContent = 'Previous';
    previous.setAttribute('aria-label', 'Previous marker');
    const position = document.createElement('span');
    position.setAttribute('aria-live', 'polite');
    position.setAttribute('aria-atomic', 'true');
    const next = document.createElement('button');
    next.type = 'button';
    next.textContent = 'Next';
    next.setAttribute('aria-label', 'Next marker');
    navigation.append(previous, position, next);
    root.append(content, navigation);

    let index = 0;
    const render = () => {
        content.innerHTML = markers[index]!.popupHtml;
        position.textContent = ` ${index + 1} of ${markers.length} `;
        previous.disabled = index === 0;
        next.disabled = index === markers.length - 1;
        if (previous.disabled && document.activeElement === previous) next.focus();
        if (next.disabled && document.activeElement === next) previous.focus();
    };
    previous.onclick = () => {
        if (index > 0) { index--; render(); }
    };
    next.onclick = () => {
        if (index < markers.length - 1) { index++; render(); }
    };
    render();
    popup.setDOMContent(root);
    popup.on('close', () => {
        previous.onclick = null;
        next.onclick = null;
    });
}

function removeMarkerLayers(map: MapLibreMap, containerId: string): void {
    markerSelections.delete(map);
    removeClusterLayers(map, MARKERS_SOURCE_ID);
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

function fitToMarkerBounds(map: MapLibreMap, markers: MarkerData[]): void {
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
