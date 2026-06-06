// ── SkyNet Map (Leaflet.js) ──────────────────────────────────────
let map, airportMarkers = {}, routeLayer = null, mstLayer = null;
let allAirports = [];

const MAJOR_HUBS = ['LHR','JFK','DXB','SIN','NRT','CDG','AMS','FRA','IST','TAS','ICN','PEK','SFO','LAX','ORD'];

async function initMap() {
  map = L.map('map', { center: [30, 20], zoom: 3, zoomControl: true });

  // Dark tile layer
  L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
    attribution: '©OpenStreetMap ©CARTO',
    subdomains: 'abcd', maxZoom: 18
  }).addTo(map);

  await loadAirports();
  document.getElementById('loading').style.display = 'none';
}

async function loadAirports() {
  try {
    allAirports = await API.airports.getAll();
    document.getElementById('airport-count').textContent = `${allAirports.length} ta aeroport`;

    allAirports.forEach(a => {
      if (!a.latitude || !a.longitude) return;
      const isHub = MAJOR_HUBS.includes(a.iataCode);
      const color = !a.isActive ? '#ef4444' : isHub ? '#f59e0b' : '#60a5fa';
      const size  = isHub ? 8 : 5;

      const marker = L.circleMarker([a.latitude, a.longitude], {
        radius: size, color, fillColor: color,
        fillOpacity: 0.85, weight: 1, opacity: 0.9
      }).addTo(map);

      marker.bindPopup(`
        <div style="font-family:'Segoe UI';min-width:160px">
          <div style="font-weight:700;font-size:1rem;color:#60a5fa">${a.iataCode}</div>
          <div style="font-size:0.85rem;margin-bottom:6px">${a.name}</div>
          <div style="font-size:0.75rem;color:#94a3b8">${a.city}, ${a.country}</div>
          <div style="font-size:0.75rem;margin-top:6px">
            Foydalanish: <b>${a.usageCount.toLocaleString()}</b>
          </div>
          <div style="margin-top:8px;display:flex;gap:6px">
            <button onclick="document.getElementById('from').value='${a.iataCode}'"
              style="padding:3px 8px;background:#1e40af;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:0.72rem">Qayerdan</button>
            <button onclick="document.getElementById('to').value='${a.iataCode}'"
              style="padding:3px 8px;background:#065f46;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:0.72rem">Qayerga</button>
          </div>
        </div>
      `);

      airportMarkers[a.iataCode] = { marker, airport: a };
    });

    // Draw some sample routes
    await drawSampleRoutes();
  } catch (e) {
    toast('Aeroportlarni yuklashda xatolik: ' + e.message, 'error');
  }
}

async function drawSampleRoutes() {
  const pairs = [
    ['TAS','LHR'],['TAS','IST'],['TAS','DXB'],['LHR','JFK'],
    ['JFK','LAX'],['DXB','SIN'],['SIN','NRT'],['NRT','ICN'],
    ['IST','CDG'],['CDG','LHR'],['LHR','DXB'],['DXB','BOM'],
  ];

  pairs.forEach(([from, to]) => {
    const a = airportMarkers[from], b = airportMarkers[to];
    if (!a || !b) return;
    L.polyline([
      [a.airport.latitude, a.airport.longitude],
      [b.airport.latitude, b.airport.longitude]
    ], { color: '#2563eb', weight: 1, opacity: 0.35, dashArray: null }).addTo(map);
  });
}

async function findRoute() {
  const from = document.getElementById('from').value.trim().toUpperCase();
  const to   = document.getElementById('to').value.trim().toUpperCase();
  if (!from || !to) { toast('Qayerdan va qayerga borishi kiritilsin.', 'error'); return; }

  clearRouteLayer();
  toast('Dijkstra yo\'li hisoblanmoqda...', 'info');

  try {
    const result = await API.flights.getRoute(from, to);
    if (!result.found) { toast(`${from} dan ${to} ga marshrut topilmadi`, 'error'); return; }

    // Draw path on map
    const latlngs = result.path
      .map(iata => airportMarkers[iata])
      .filter(Boolean)
      .map(m => [m.airport.latitude, m.airport.longitude]);

    routeLayer = L.polyline(latlngs, { color: '#22c55e', weight: 3, opacity: 0.9 }).addTo(map);
    map.fitBounds(routeLayer.getBounds(), { padding: [60, 60] });

    // Highlight path airports
    result.path.forEach((iata, i) => {
      const m = airportMarkers[iata];
      if (!m) return;
      m.marker.setStyle({ color: '#22c55e', fillColor: '#22c55e', radius: 10 });
    });

    // Show result panel
    const el = document.getElementById('routeResult');
    document.getElementById('routeStats').textContent =
      `${Math.round(result.totalDistance).toLocaleString()} km  ·  $${Math.round(result.totalCost)}  ·  ${result.stops} ta to'xtash`;

    const pathEl = document.getElementById('routePath');
    pathEl.innerHTML = result.path.map((n, i) =>
      `${i > 0 ? '<span class="arrow">→</span>' : ''}<span class="node">${n}</span>`
    ).join('');

    el.style.display = 'block';
    toast(`Marshrut topildi: ${result.path.join(' → ')}`, 'success');
  } catch (e) {
    toast('Xatolik: ' + e.message, 'error');
  }
}

async function showMST() {
  clearMSTLayer();
  toast('Kruskal MST hisoblanmoqda...', 'info');

  try {
    const result = await API.flights.getMST();
    const lines = [];

    result.edges.forEach(edge => {
      const a = airportMarkers[edge.from], b = airportMarkers[edge.to];
      if (!a || !b) return;
      lines.push([
        [a.airport.latitude, a.airport.longitude],
        [b.airport.latitude, b.airport.longitude]
      ]);
    });

    mstLayer = L.layerGroup(lines.map(pts =>
      L.polyline(pts, { color: '#f59e0b', weight: 1.5, opacity: 0.6, dashArray: '6,4' })
    )).addTo(map);

    toast(`MST: ${result.edgeCount} ta qirra, ${Math.round(result.totalWeight).toLocaleString()} km jami`, 'success');
  } catch (e) {
    toast('Xatolik: ' + e.message, 'error');
  }
}

async function closeAirport() {
  const iata = document.getElementById('closeAirport').value.trim().toUpperCase();
  if (!iata) return;
  try {
    await API.airports.close(iata);
    const m = airportMarkers[iata];
    if (m) m.marker.setStyle({ color: '#ef4444', fillColor: '#ef4444' });
    toast(`${iata} aeroporti yopildi (Favqulodda rejim)`, 'error');
  } catch (e) { toast('Xatolik: ' + e.message, 'error'); }
}

function clearRouteLayer() {
  if (routeLayer) { routeLayer.remove(); routeLayer = null; }
  document.getElementById('routeResult').style.display = 'none';
  // Reset marker styles
  Object.values(airportMarkers).forEach(({ marker, airport }) => {
    const isHub = MAJOR_HUBS.includes(airport.iataCode);
    marker.setStyle({
      color: airport.isActive ? (isHub ? '#f59e0b' : '#60a5fa') : '#ef4444',
      fillColor: airport.isActive ? (isHub ? '#f59e0b' : '#60a5fa') : '#ef4444',
      radius: isHub ? 8 : 5
    });
  });
}

function clearMSTLayer() {
  if (mstLayer) { mstLayer.remove(); mstLayer = null; }
}

function clearMap() { clearRouteLayer(); clearMSTLayer(); }

// SignalR handlers
function onEmergencyAlert(data) {
  const m = airportMarkers[data.airport];
  if (m) m.marker.setStyle({ color: '#ef4444', fillColor: '#ef4444' });
}

// ── Autocomplete for map panel inputs ────────────────────────────
let acTimer = null;

async function mapAC(inputId, dropId) {
  const q    = document.getElementById(inputId).value.trim();
  const drop = document.getElementById(dropId);
  if (q.length < 2) { drop.style.display = 'none'; return; }

  clearTimeout(acTimer);
  acTimer = setTimeout(async () => {
    try {
      const results = await API.airports.search(q);
      if (!results || !results.length) { drop.style.display = 'none'; return; }
      drop.innerHTML = results.map(a => `
        <div class="ac-item" onmousedown="pickAC('${inputId}','${dropId}','${a.iataCode}')">
          <span class="ac-code">${a.iataCode}</span>
          <span style="color:#94a3b8">${a.name.length > 28 ? a.name.slice(0,28)+'…' : a.name}</span>
        </div>
      `).join('');
      drop.style.display = 'block';
    } catch(e) { drop.style.display = 'none'; }
  }, 200);
}

function pickAC(inputId, dropId, iata) {
  document.getElementById(inputId).value = iata;
  document.getElementById(dropId).style.display = 'none';
}

function acKey(e, inputId, dropId) {
  if (e.key === 'Enter') {
    document.getElementById(dropId).style.display = 'none';
    findRoute();
  } else if (e.key === 'Escape') {
    document.getElementById(dropId).style.display = 'none';
  }
}

document.addEventListener('click', e => {
  if (!e.target.closest('.ac-wrap')) {
    document.querySelectorAll('.ac-dropdown').forEach(d => d.style.display = 'none');
  }
});

document.addEventListener('DOMContentLoaded', initMap);
