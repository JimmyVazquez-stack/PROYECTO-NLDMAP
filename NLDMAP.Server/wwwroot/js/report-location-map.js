const maps = new WeakMap();
const destroyed = new WeakSet();
const leafletBase = "https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/";
let leafletLoading;

function loadResource(element, url, integrity) {
    return new Promise((resolve, reject) => {
        element.integrity = integrity;
        element.crossOrigin = "anonymous";
        if (element instanceof HTMLLinkElement) {
            element.rel = "stylesheet";
            element.href = url;
        } else {
            element.src = url;
        }

        element.onload = resolve;
        element.onerror = () => {
            element.remove();
            reject(new Error("No se pudo cargar Leaflet."));
        };
        document.head.append(element);
    });
}

function loadLeaflet() {
    leafletLoading ??= Promise.all([
        loadResource(document.createElement("link"), leafletBase + "leaflet.css",
            "sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="),
        loadResource(document.createElement("script"), leafletBase + "leaflet.js",
            "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=")
    ]).then(() => window.L);
    return leafletLoading;
}

export async function initialize(element, callbacks, cluster, analysisCallbacks) {
    const L = await loadLeaflet();
    if (!element?.isConnected || destroyed.has(element) || maps.has(element)) return;

    const map = L.map(element, { scrollWheelZoom: false }).setView([19.32, -98.24], 10);
    L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);
    const points = L.layerGroup().addTo(map);
    const search = L.layerGroup().addTo(map);
    const observer = new ResizeObserver(() => map.invalidateSize({ pan: false }));
    observer.observe(element);

    if (callbacks) {
        map.on("click", ({ latlng }) => {
            const { lat, lng } = latlng.wrap();
            callbacks.invokeMethodAsync("SelectPoint", Number(lat.toFixed(6)), Number(lng.toFixed(6)))
                .catch(error => console.warn("No se pudo seleccionar el punto.", error));
        });
    }

    const state = { L, map, points, search, observer, editable: Boolean(callbacks),
        cluster, pins: [], hasPoints: false, area: null };
    maps.set(element, state);
    if (analysisCallbacks) {
        state.selectReports = group => analysisCallbacks.invokeMethodAsync("SelectReports", group.map(pin => pin.folio))
            .catch(error => console.warn("No se pudieron seleccionar los reportes.", error));
        state.canvas = document.createElement("canvas");
        state.canvas.className = "analysis-heat";
        state.canvas.setAttribute("aria-hidden", "true");
        state.canvas.hidden = true;
        element.append(state.canvas);
        state.density = document.createElement("canvas");
        map.on("moveend resize", () => renderAnalysis(state));
    } else if (cluster) {
        map.on("zoomend", () => renderPoints(state));
    }
}

export function setPoints(element, pins, area) {
    const state = maps.get(element);
    if (!state) return;

    const { L, map, search } = state;
    const sameArea = state.area?.latitude === area?.latitude &&
        state.area?.longitude === area?.longitude && state.area?.kilometers === area?.kilometers;
    const previousArea = state.area;
    state.area = area;
    state.pins = pins;
    if (!sameArea) {
        search.clearLayers();
        if (area) {
            const center = [area.latitude, area.longitude];
            const circle = L.circle(center, {
                radius: area.kilometers * 1000, color: "#146daf", weight: 2,
                fillOpacity: 0.1, interactive: false
            }).addTo(search);
            L.circleMarker(center, { radius: 4, color: "#146daf", interactive: false }).addTo(search);
            if (!previousArea) map.fitBounds(circle.getBounds(), { padding: [18, 18], maxZoom: 15 });
        }
    }

    if (!area) {
        const positions = pins.map(pin => [pin.latitude, pin.longitude]);
        if (positions.length === 1 && (!state.editable || !state.hasPoints || state.cluster)) {
            if (state.cluster || !state.editable || !map.getBounds().contains(positions[0]))
                map.setView(positions[0], 14);
        } else if (positions.length > 1) {
            map.fitBounds(positions, { padding: [24, 24], maxZoom: 14 });
        }
    }

    state.hasPoints = pins.length > 0;
    renderPoints(state);
}

function renderPoints({ L, map, points, pins, cluster }) {
    points.clearLayers();
    const groups = cluster ? groupByPixel(map, pins) : pins.map(pin => [pin]);
    for (const group of groups) {
        if (group.length === 1) {
            const pin = group[0];
            const marker = L.circleMarker([pin.latitude, pin.longitude], {
                radius: 8, color: "#fff", weight: 2, fillColor: "#126c61", fillOpacity: 1, bubblingMouseEvents: false
            }).addTo(points);
            if (pin.label) {
                const label = document.createElement("span");
                label.textContent = pin.label;
                marker.bindPopup(label);
            }
            continue;
        }

        const positions = group.map(pin => [pin.latitude, pin.longitude]);
        const bounds = L.latLngBounds(positions);
        const count = document.createElement("span");
        count.textContent = String(group.length);
        const marker = L.marker(bounds.getCenter(), {
            icon: L.divIcon({ className: "report-map-cluster", html: count, iconSize: [38, 38] }),
            title: `${group.length} reportes`,
            bubblingMouseEvents: false
        }).addTo(points);

        if (map.getZoom() < 19 && !bounds.getSouthWest().equals(bounds.getNorthEast())) {
            marker.on("click", () => {
                const zoom = map.getBoundsZoom(bounds, false, [76, 76]);
                if (zoom > map.getZoom()) {
                    map.fitBounds(bounds, { padding: [38, 38], maxZoom: 19 });
                } else {
                    map.setView(bounds.getCenter(), Math.min(19, map.getZoom() + 2));
                }
            });
        } else {
            const list = document.createElement("ul");
            for (const pin of group.slice(0, 15)) {
                const item = document.createElement("li");
                item.textContent = pin.label || "Reporte sin etiqueta";
                list.append(item);
            }
            if (group.length > 15) {
                const item = document.createElement("li");
                item.textContent = `y ${group.length - 15} más`;
                list.append(item);
            }
            marker.bindPopup(list);
        }
    }
}

export function setAnalysisPoints(element, pins, heat, fit) {
    const state = maps.get(element);
    if (!state?.selectReports) return;
    state.pins = pins;
    state.heat = heat;
    if (fit && pins.length) {
        state.map.fitBounds(pins.map(pin => [pin.latitude, pin.longitude]), { padding: [28, 28], maxZoom: 14 });
    }
    renderAnalysis(state);
}

function renderAnalysis(state) {
    const { L, map, points, pins, canvas, selectReports, heat } = state;
    points.clearLayers();
    canvas.hidden = !heat;
    if (heat) {
        renderHeat(state);
        return;
    }
    for (const group of groupByPixel(map, pins)) {
        const position = L.latLngBounds(group.map(pin => [pin.latitude, pin.longitude])).getCenter();
        const single = group.length === 1;
        const count = document.createElement("span");
        count.textContent = String(group.length);
        const icon = single
            ? L.divIcon({ className: "analysis-point", iconSize: [16, 16] })
            : L.divIcon({ className: "report-map-cluster", html: count, iconSize: [38, 38] });
        const title = single ? group[0].folio : `${group.length} reportes`;
        L.marker(position, { icon, title, alt: title }).addTo(points).on("click", () => selectReports(group));
    }
}

const heatColors = [[37, 67, 191], [0, 183, 206], [255, 224, 101], [202, 42, 38]];

function renderHeat({ map, pins, canvas, density }) {
    const size = map.getSize();
    if (!size.x || !size.y) return;

    canvas.width = density.width = size.x;
    canvas.height = density.height = size.y;
    const field = density.getContext("2d", { willReadFrequently: true });
    field.globalCompositeOperation = "lighter";
    const radius = 28;
    for (const pin of pins) {
        const { x, y } = map.latLngToContainerPoint([pin.latitude, pin.longitude]);
        const gradient = field.createRadialGradient(x, y, 0, x, y, radius);
        gradient.addColorStop(0, "rgba(255,255,255,.22)");
        gradient.addColorStop(1, "rgba(255,255,255,0)");
        field.fillStyle = gradient;
        field.fillRect(x - radius, y - radius, radius * 2, radius * 2);
    }

    const pixels = field.getImageData(0, 0, size.x, size.y);
    for (let index = 0; index < pixels.data.length; index += 4) {
        const alpha = pixels.data[index + 3];
        if (!alpha) continue;
        const strength = alpha * 3 / 255;
        const lower = Math.min(2, Math.floor(strength));
        for (let channel = 0; channel < 3; channel++) {
            const start = heatColors[lower][channel];
            pixels.data[index + channel] = start + (heatColors[lower + 1][channel] - start) * (strength - lower);
        }
        pixels.data[index + 3] = Math.min(210, alpha * 3);
    }
    canvas.getContext("2d").putImageData(pixels, 0, 0);
}

function groupByPixel(map, pins) {
    const groups = new Map();
    const zoom = map.getZoom();
    const cellSize = zoom >= 19 ? 22 : 56;
    for (const pin of pins) {
        const pixel = map.project([pin.latitude, pin.longitude], zoom);
        const key = `${Math.floor(pixel.x / cellSize)}:${Math.floor(pixel.y / cellSize)}`;
        if (!groups.has(key)) groups.set(key, []);
        groups.get(key).push(pin);
    }
    return [...groups.values()];
}

export function destroy(element) {
    if (!element) return;
    destroyed.add(element);
    const state = maps.get(element);
    if (!state) return;

    state.observer.disconnect();
    state.canvas?.remove();
    state.map.remove();
    maps.delete(element);
}
