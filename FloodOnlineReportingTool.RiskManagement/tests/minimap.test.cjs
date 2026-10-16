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

function createMap({ inTab = false, hidden = false } = {}) {
    const instances = [];
    const observers = [];
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
        on() {}
        loaded() { return false; }
        addSource(id, source) { this.sources.set(id, source); }
        getSource(id) { return this.sources.get(id); }
        removeSource(id) { this.sources.delete(id); }
        addLayer(layer) { this.layers.set(layer.id, layer); }
        getLayer(id) { return this.layers.get(id); }
        removeLayer(id) { this.layers.delete(id); }
        fitBounds(bounds) { this.fittedBounds = bounds; }
        flyTo(options) { this.camera = options; }
        resize() { this.resizeCount++; }
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
        console,
    });

    exports.initMiniMap('map', 'test-key', 'OS', [330000, 67000, 423000, 133000], [
        { id: 'overlay', url: 'https://wms.example/', layers: 'flood', format: 'image/png' },
    ]);
    return {
        api: exports,
        map: instances[0],
        observers,
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
    const point = map.sources.get('mini-map-geometry').data.coordinates;
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
