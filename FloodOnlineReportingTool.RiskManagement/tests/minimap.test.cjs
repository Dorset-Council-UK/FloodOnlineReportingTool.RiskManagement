const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');
const ts = require('typescript');

const source = fs.readFileSync(path.join(__dirname, '../Components/MiniMap.razor.ts'), 'utf8');
const compiled = ts.transpileModule(source, {
    compilerOptions: {
        module: ts.ModuleKind.CommonJS,
        target: ts.ScriptTarget.ES2022,
        esModuleInterop: true,
    },
}).outputText;

function createMap({ inTab = false, hidden = false, useClustering } = {}) {
    const instances = [];
    const observers = [];
    const popups = [];
    const warnings = [];
    let workerUrl;
    let workerConfigurations = 0;
    const panel = { classList: { contains: () => hidden } };
    const container = { closest: () => inTab ? panel : null };

    class FakeMap {
        constructor(options) {
            assert.equal(workerUrl, 'https://reports.example/app/js/workers/maplibre-gl-worker.mjs');
            this.options = options;
            this.sources = new Map();
            this.layers = new Map();
            this.events = new Map();
            this.handlers = new Map();
            this.canvas = { style: { cursor: '' } };
            this.cameraChanges = 0;
            this.resizeCount = 0;
            instances.push(this);
        }
        addControl() {}
        once(event, callback) {
            const callbacks = this.events.get(event) ?? [];
            callbacks.push(callback);
            this.events.set(event, callbacks);
        }
        emit(event) {
            const callbacks = this.events.get(event) ?? [];
            this.events.delete(event);
            callbacks.forEach(callback => callback());
        }
        on(event, layerId, callback) {
            const key = `${event}:${layerId}`;
            const callbacks = this.handlers.get(key) ?? [];
            callbacks.push(callback);
            this.handlers.set(key, callbacks);
        }
        trigger(event, layerId, data = {}) {
            for (const callback of this.handlers.get(`${event}:${layerId}`) ?? []) callback(data);
        }
        getCanvas() { return this.canvas; }
        getStyle() {
            return { layers: [{ type: 'symbol', layout: { 'text-font': ['Basemap Regular'] } }] };
        }
        loaded() { return false; }
        addSource(id, source) {
            assert.equal(this.sources.has(id), false);
            this.sources.set(id, {
                ...source,
                getClusterExpansionZoom: async clusterId => {
                    this.expandedClusterId = clusterId;
                    return 15;
                },
            });
        }
        getSource(id) { return this.sources.get(id); }
        removeSource(id) {
            assert.ok([...this.layers.values()].every(layer => layer.source !== id));
            this.sources.delete(id);
        }
        addLayer(layer) {
            assert.equal(this.layers.has(layer.id), false);
            assert.ok(this.sources.has(layer.source));
            this.layers.set(layer.id, layer);
        }
        getLayer(id) { return this.layers.get(id); }
        removeLayer(id) { this.layers.delete(id); }
        fitBounds(bounds) { this.fittedBounds = bounds; this.cameraChanges++; }
        flyTo(options) { this.camera = options; this.cameraChanges++; }
        easeTo(options) { this.camera = options; this.cameraChanges++; }
        resize() { this.resizeCount++; }
        remove() { this.removed = true; }
    }

    class FakePopup {
        constructor(options) { this.options = options; popups.push(this); }
        setLngLat(coordinates) { this.coordinates = coordinates; return this; }
        setHTML(html) { this.html = html; return this; }
        addTo(map) { this.map = map; return this; }
        remove() { this.removed = true; }
    }

    class FakeObserver {
        constructor(callback) {
            this.callback = callback;
            observers.push(this);
        }
        observe() { this.active = true; }
        disconnect() { this.active = false; }
    }

    const exports = {};
    vm.runInNewContext(compiled, {
        exports,
        require: name => {
            if (name === 'maplibre-gl') {
                return {
                    Map: FakeMap,
                    AttributionControl: class {},
                    NavigationControl: class {},
                    Popup: FakePopup,
                    setWorkerUrl: url => { workerUrl = url; workerConfigurations++; },
                };
            }
            if (name.endsWith('.css')) return {};
            return require(name);
        },
        document: {
            baseURI: 'https://reports.example/app/',
            getElementById: () => container,
        },
        MutationObserver: FakeObserver,
        URL,
        console: { ...console, warn: (...args) => warnings.push(args) },
    });

    exports.initMiniMap('map', 'test-key', 'OS', [330000, 67000, 423000, 133000], [
        { id: 'overlay', url: 'https://wms.example/', layers: 'flood', format: 'image/png' },
    ], useClustering);
    return {
        api: exports,
        map: instances[0],
        observers,
        instances,
        popups,
        warnings,
        getWorkerConfigurations: () => workerConfigurations,
        setHidden(value) {
            hidden = value;
            observers.filter(observer => observer.active).forEach(observer => observer.callback());
        },
    };
}

const geometry = JSON.stringify({ type: 'Point', coordinates: [370000, 90000] });
const markers = JSON.stringify([
    { id: 'report', latitude: 50.7, longitude: -2.4, popupHtml: 'Report' },
]);

test('publishes the installed MapLibre worker at the configured static asset path', () => {
    const config = require('../webpack.common.js');
    const copyPlugin = config.plugins.find(plugin => plugin.constructor.name === 'CopyPlugin');
    const worker = copyPlugin.patterns.find(pattern => pattern.to === 'js/workers/maplibre-gl-worker.mjs');
    assert.equal(worker.from, require.resolve('maplibre-gl/dist/maplibre-gl-worker.mjs'));
    assert.ok(fs.existsSync(worker.from));
    const shared = copyPlugin.patterns.find(pattern => pattern.to === 'js/workers/maplibre-gl-shared.mjs');
    assert.equal(shared.from, require.resolve('maplibre-gl/dist/maplibre-gl-shared.mjs'));
    for (const asset of [worker, shared]) {
        assert.equal(asset.info.minimized, true);
        const moduleSource = fs.readFileSync(asset.from, 'utf8');
        const imports = moduleSource.matchAll(/(?:from\s*|import\s*)["'](\.[^"']+)["']/g);
        for (const [, dependency] of imports) {
            const destination = path.posix.join(path.posix.dirname(asset.to), dependency);
            assert.ok(copyPlugin.patterns.some(pattern => pattern.to === destination),
                `Missing worker dependency: ${destination}`);
        }
    }
});

test('configures the worker once before constructing multiple maps', () => {
    const { api, getWorkerConfigurations } = createMap();
    api.initMiniMap('second-map', 'test-key', 'OS', [330000, 67000, 423000, 133000], []);
    assert.equal(getWorkerConfigurations(), 1);
});

test('authenticates dependent OS resources without sending the key to other hosts', () => {
    const { map } = createMap();
    for (const resource of ['tile/1/2/3.pbf', 'resources/fonts/font/0-255.pbf', 'resources/sprites/sprite.json']) {
        const request = map.options.transformRequest(`https://api.os.uk/maps/vector/v1/vts/${resource}?srs=3857&key=old`);
        const url = new URL(request.url);
        assert.equal(url.searchParams.get('key'), 'test-key');
        assert.equal(url.searchParams.getAll('key').length, 1);
        assert.equal(url.searchParams.get('srs'), '3857');
    }
    for (const url of ['https://wms.example/tiles', 'https://api.os.uk.example/tiles', '/local/tiles']) {
        assert.equal(map.options.transformRequest(url).url, url);
    }
});

test('initializes an empty map at the projected Dorset default bounds', () => {
    const { map } = createMap();
    const [west, south, east, north] = map.options.bounds;
    assert.ok(west > -4 && west < -2);
    assert.ok(east > -2 && east < 0);
    assert.ok(south > 50 && south < 51);
    assert.ok(north > south && north < 52);
    assert.equal(new URL(map.options.style).searchParams.get('srs'), '3857');
    assert.equal(map.resizeCount, 1);
});

test('renders queued geometry, markers and WMS when the style is ready, without full load', () => {
    const { api, map } = createMap();
    api.updateGeometry('map', geometry, true);
    api.updateMarkers('map', markers);
    assert.equal(map.sources.size, 0);

    map.emit('style.load');

    assert.equal(map.loaded(), false);
    assert.ok(map.sources.has('overlay'));
    const point = map.sources.get('mini-map-geometry-points').data.features[0].geometry.coordinates;
    assert.ok(point[0] > -4 && point[0] < 0);
    assert.ok(point[1] > 50 && point[1] < 52);
    const marker = map.sources.get('mini-map-markers').data.features[0];
    assert.equal(marker.geometry.coordinates[0], -2.4);
    assert.equal(marker.geometry.coordinates[1], 50.7);
    assert.equal(map.events.has('load'), false);
});

test('applies later updates while the basemap is still loading and supports clearing', () => {
    const { api, map } = createMap();
    map.emit('style.load');
    api.updateGeometry('map', geometry, false);
    api.updateMarkers('map', markers);
    assert.ok(map.sources.has('mini-map-geometry'));
    assert.ok(map.sources.has('mini-map-markers'));
    api.updateGeometry('map', null, false);
    api.updateMarkers('map', null);
    assert.equal(map.sources.has('mini-map-geometry'), false);
    assert.equal(map.sources.has('mini-map-markers'), false);
});

test('resizes on every tab activation but notifies .NET only once', () => {
    const { api, map, setHidden, observers } = createMap({ inTab: true, hidden: true });
    let notifications = 0;
    api.observeVisibility('map', { invokeMethodAsync: async () => { notifications++; } }, 'Visible');
    assert.equal(map.resizeCount, 0);
    assert.equal(notifications, 0);
    setHidden(false);
    assert.equal(map.resizeCount, 1);
    assert.equal(notifications, 1);
    setHidden(true);
    setHidden(false);
    assert.equal(map.resizeCount, 2);
    assert.equal(notifications, 1);
    api.destroyMap('map');
    assert.ok(map.removed);
    assert.ok(observers.every(observer => !observer.active));
    setHidden(false);
    assert.equal(notifications, 1);
});

test('resizes hidden-tab maps even without a .NET visibility callback', () => {
    const { map, setHidden } = createMap({ inTab: true, hidden: true });
    setHidden(false);
    assert.equal(map.resizeCount, 1);
});

const plain = value => JSON.parse(JSON.stringify(value));
const flush = () => new Promise(resolve => setImmediate(resolve));
const clusterEvent = {
    features: [{ geometry: { type: 'Point', coordinates: [-2.4, 50.7] }, properties: { cluster_id: 42 } }],
};

test('clusters both point sources by default with count labels and basemap fonts', () => {
    const { api, map } = createMap();
    map.emit('style.load');
    api.updateGeometry('map', geometry, true);
    api.updateMarkers('map', markers);
    for (const sourceId of ['mini-map-markers', 'mini-map-geometry-points']) {
        const source = map.getSource(sourceId);
        assert.equal(source.cluster, true);
        assert.equal(source.clusterRadius, 50);
        assert.equal(source.clusterMaxZoom, 14);
        assert.equal(source.clusterMinPoints, 2);
        assert.equal(source.data.features[0].geometry.type, 'Point');
        const circles = map.getLayer(`${sourceId}-clusters`);
        assert.deepEqual(plain(circles.filter), ['has', 'point_count']);
        assert.equal(circles.paint['circle-radius'][0], 'step');
        const label = map.getLayer(`${sourceId}-cluster-count`);
        assert.deepEqual(plain(label.layout['text-field']), ['get', 'point_count_abbreviated']);
        assert.deepEqual(plain(label.layout['text-font']), ['Basemap Regular']);
        assert.equal(label.paint['text-color'], '#ffffff');
    }
    for (const layerId of ['mini-map-markers-circle', 'mini-map-geometry-circle']) {
        assert.deepEqual(plain(map.getLayer(layerId).filter), ['!', ['has', 'point_count']]);
    }
    assert.equal(map.getSource('mini-map-geometry').cluster, undefined);
    assert.equal(map.getSource('overlay').type, 'raster');
});

test('disabled clustering preserves the original geometry and marker layers', () => {
    const { api, map } = createMap({ useClustering: false });
    map.emit('style.load');
    api.updateGeometry('map', geometry, true);
    api.updateMarkers('map', markers);
    assert.equal(map.getSource('mini-map-geometry').data.type, 'Point');
    assert.equal(map.getSource('mini-map-geometry-points'), undefined);
    assert.equal(map.getSource('mini-map-markers').cluster, undefined);
    assert.equal(map.getLayer('mini-map-markers-clusters'), undefined);
    assert.equal(map.getLayer('mini-map-geometry-circle').source, 'mini-map-geometry');
    assert.equal(map.getLayer('mini-map-markers-circle').paint['circle-color'], '#d4351c');
    assert.equal(map.getLayer('mini-map-geometry-circle').paint['circle-color'], '#1d70b8');
});

test('extracts MultiPoints and nested collection points without losing non-point geometries or metadata', () => {
    const { api, map } = createMap();
    map.emit('style.load');
    const mixed = JSON.stringify({
        type: 'FeatureCollection',
        features: [
            { type: 'Feature', id: 'point', properties: { name: 'point' }, geometry: JSON.parse(geometry) },
            { type: 'Feature', id: 'multi', properties: { name: 'multi' }, geometry: {
                type: 'MultiPoint', coordinates: [[370000, 90000, 5], [370100, 90100]],
            } },
            { type: 'Feature', id: 'mixed', properties: { name: 'mixed' }, geometry: {
                type: 'GeometryCollection', geometries: [
                    JSON.parse(geometry),
                    { type: 'GeometryCollection', geometries: [
                        JSON.parse(geometry),
                        { type: 'LineString', coordinates: [[370000, 90000], [400000, 120000]] },
                    ] },
                    { type: 'Polygon', coordinates: [[[370000, 90000], [370000, 91000], [371000, 90000], [370000, 90000]]] },
                ],
            } },
            { type: 'Feature', properties: {}, geometry: null },
        ],
    });
    api.updateGeometry('map', mixed, true);
    const points = map.getSource('mini-map-geometry-points').data.features;
    assert.equal(points.length, 5);
    assert.ok(points.every(feature => feature.geometry.type === 'Point'));
    assert.equal(new Set(points.map(feature => feature.id)).size, 5);
    assert.equal(points[0].id, 'point');
    assert.equal(points[1].properties.name, 'multi');
    assert.equal(points[1].geometry.coordinates[2], 5);
    assert.ok(points.every(feature => feature.geometry.coordinates[0] > -4 && feature.geometry.coordinates[0] < 0));
    const other = map.getSource('mini-map-geometry').data.features;
    assert.equal(other.length, 1);
    assert.equal(other[0].id, 'mixed');
    assert.equal(other[0].properties.name, 'mixed');
    assert.equal(other[0].geometry.geometries[0].geometries[0].type, 'LineString');
    assert.equal(other[0].geometry.geometries[1].type, 'Polygon');
    assert.equal(map.getLayer('mini-map-geometry-fill').source, 'mini-map-geometry');
    assert.equal(map.getLayer('mini-map-geometry-line').source, 'mini-map-geometry');

    const bounds = plain(map.fittedBounds);
    api.updateClustering('map', false);
    api.updateGeometry('map', mixed, true);
    assert.deepEqual(plain(map.fittedBounds), bounds);
    assert.equal(map.getSource('mini-map-geometry').data.features.length, 4);
});

test('accepts a bare GeometryCollection and leaves non-point-only data unclustered', () => {
    const { api, map } = createMap();
    map.emit('style.load');
    api.updateGeometry('map', JSON.stringify({ type: 'GeometryCollection', geometries: [
        { type: 'MultiPoint', coordinates: [[370000, 90000], [370100, 90100]] },
        { type: 'MultiLineString', coordinates: [[[370000, 90000], [371000, 91000]]] },
    ] }), false);
    assert.equal(map.getSource('mini-map-geometry-points').data.features.length, 2);
    assert.equal(map.getSource('mini-map-geometry').data.features[0].geometry.geometries[0].type, 'MultiLineString');
    api.updateGeometry('map', JSON.stringify({ type: 'LineString', coordinates: [[370000, 90000], [371000, 91000]] }), false);
    assert.equal(map.getSource('mini-map-geometry-points').data.features.length, 0);
    assert.equal(map.getSource('mini-map-geometry').cluster, undefined);
    assert.equal(map.getSource('mini-map-geometry').data.features[0].geometry.type, 'LineString');
});

test('preserves individual marker popups and closes them on rebuild or destruction', () => {
    const { api, map, popups } = createMap();
    map.emit('style.load');
    api.updateMarkers('map', markers);
    const click = () => map.trigger('click', 'mini-map-markers-circle', {
        features: map.getSource('mini-map-markers').data.features,
    });
    map.trigger('mouseenter', 'mini-map-markers-circle');
    assert.equal(map.canvas.style.cursor, 'pointer');
    click();
    assert.equal(popups[0].html, 'Report');
    assert.deepEqual(plain(popups[0].coordinates), [-2.4, 50.7]);
    click();
    assert.equal(popups[0].removed, true);
    api.updateClustering('map', false);
    assert.equal(popups[1].removed, true);
    assert.equal(map.canvas.style.cursor, '');
    click();
    api.destroyMap('map');
    assert.equal(popups[2].removed, true);
});

test('toggles clustering using retained imperative data without moving the camera or duplicating handlers', () => {
    const { api, map } = createMap();
    map.emit('style.load');
    api.updateGeometry('map', geometry, true);
    api.updateMarkers('map', markers);
    const cameraChanges = map.cameraChanges;
    const source = map.getSource('mini-map-markers');
    api.updateClustering('map', true);
    assert.equal(map.getSource('mini-map-markers'), source);
    for (let index = 0; index < 3; index++) {
        api.updateClustering('map', false);
        assert.equal(map.getSource('mini-map-markers').cluster, undefined);
        assert.equal(map.getSource('mini-map-geometry-points'), undefined);
        api.updateClustering('map', true);
        assert.equal(map.getSource('mini-map-markers').data.features.length, 1);
        assert.equal(map.getSource('mini-map-geometry-points').data.features.length, 1);
    }
    assert.equal(map.cameraChanges, cameraChanges);
    assert.ok([...map.handlers.values()].every(callbacks => callbacks.length === 1));
});

test('expands clusters from both point sources and provides hover feedback', async () => {
    const { api, map, popups } = createMap();
    map.emit('style.load');
    api.updateGeometry('map', geometry, false);
    api.updateMarkers('map', markers);
    for (const sourceId of ['mini-map-markers', 'mini-map-geometry-points']) {
        const layerId = `${sourceId}-clusters`;
        map.trigger('mouseenter', layerId);
        assert.equal(map.canvas.style.cursor, 'pointer');
        map.trigger('mouseleave', layerId);
        assert.equal(map.canvas.style.cursor, '');
        map.trigger('click', layerId, clusterEvent);
        await flush();
        assert.equal(map.expandedClusterId, 42);
        assert.deepEqual(plain(map.camera), { center: [-2.4, 50.7], zoom: 15 });
    }
    assert.equal(popups.length, 0);
});

test('ignores expansion results after data replacement, a toggle, or map disposal', async () => {
    for (const action of ['replace', 'toggle', 'destroy']) {
        const { api, map } = createMap();
        map.emit('style.load');
        api.updateMarkers('map', markers);
        let finish;
        map.getSource('mini-map-markers').getClusterExpansionZoom = () => new Promise(resolve => { finish = resolve; });
        map.trigger('click', 'mini-map-markers-clusters', clusterEvent);
        if (action === 'replace') api.updateMarkers('map', markers);
        if (action === 'toggle') api.updateClustering('map', false);
        if (action === 'destroy') api.destroyMap('map');
        const count = map.cameraChanges;
        finish(16);
        await flush();
        assert.equal(map.cameraChanges, count);
    }
});

test('handles rejected expansion requests and ignores out-of-order clicks', async () => {
    const { api, map, warnings } = createMap();
    map.emit('style.load');
    api.updateMarkers('map', markers);
    const source = map.getSource('mini-map-markers');
    source.getClusterExpansionZoom = async () => { throw new Error('Worker unavailable'); };
    map.trigger('click', 'mini-map-markers-clusters', clusterEvent);
    await flush();
    assert.equal(warnings.length, 1);
    const pending = [];
    source.getClusterExpansionZoom = () => new Promise(resolve => pending.push(resolve));
    map.trigger('click', 'mini-map-markers-clusters', clusterEvent);
    map.trigger('click', 'mini-map-markers-clusters', clusterEvent);
    pending[1](16);
    await flush();
    pending[0](12);
    await flush();
    assert.equal(map.camera.zoom, 16);
});

test('uses the latest pre-style data and clustering option and ignores disposed queued work', () => {
    const { api, map } = createMap();
    api.updateGeometry('map', geometry, true);
    api.updateGeometry('map', null, false);
    api.updateMarkers('map', markers);
    api.updateMarkers('map', JSON.stringify([{ id: 'latest', longitude: -2, latitude: 51, popupHtml: 'Latest' }]));
    api.updateClustering('map', false);
    map.emit('style.load');
    assert.equal(map.getSource('mini-map-geometry'), undefined);
    assert.equal(map.getSource('mini-map-markers').cluster, undefined);
    assert.equal(map.getSource('mini-map-markers').data.features[0].properties.id, 'latest');

    const disposed = createMap();
    disposed.api.updateGeometry('map', geometry, true);
    disposed.api.updateMarkers('map', markers);
    disposed.api.destroyMap('map');
    disposed.map.emit('style.load');
    assert.equal(disposed.map.sources.size, 0);
});

test('keeps clustering state independent between maps', () => {
    const { api, map, instances } = createMap();
    api.initMiniMap('second', 'test-key', 'OS', [330000, 67000, 423000, 133000], [], false);
    const second = instances[1];
    map.emit('style.load');
    second.emit('style.load');
    api.updateMarkers('map', markers);
    api.updateMarkers('second', markers);
    assert.equal(map.getSource('mini-map-markers').cluster, true);
    assert.equal(second.getSource('mini-map-markers').cluster, undefined);
    api.updateClustering('second', true);
    api.updateClustering('map', false);
    assert.equal(map.getSource('mini-map-markers').cluster, undefined);
    assert.equal(second.getSource('mini-map-markers').cluster, true);
});

test('clears all clustering layers and sources for empty, null, and malformed updates', () => {
    const { api, map, warnings } = createMap();
    map.emit('style.load');
    for (const empty of ['[]', null, 'invalid']) {
        api.updateMarkers('map', markers);
        api.updateMarkers('map', empty);
        assert.equal(map.getSource('mini-map-markers'), undefined);
        assert.equal(map.getLayer('mini-map-markers-clusters'), undefined);
        assert.equal(map.getLayer('mini-map-markers-cluster-count'), undefined);
    }
    for (const empty of [null, 'invalid']) {
        api.updateGeometry('map', geometry, true);
        api.updateGeometry('map', empty, false);
        assert.equal(map.getSource('mini-map-geometry-points'), undefined);
        assert.equal(map.getLayer('mini-map-geometry-points-clusters'), undefined);
        assert.equal(map.getLayer('mini-map-geometry-points-cluster-count'), undefined);
    }
    assert.equal(warnings.length, 2);
    api.updateClustering('map', false);
    assert.equal(map.sources.size, 1);
    assert.equal(warnings.length, 4);
});
