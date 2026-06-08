// ── SkyNet API wrapper ────────────────────────────────────────────
const API = {
  base: '',

  async get(url) {
    const res = await fetch(this.base + url);
    if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
    return res.json();
  },

  async post(url, data) {
    const res = await fetch(this.base + url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
    return res.json();
  },

  async patch(url) {
    const res = await fetch(this.base + url, { method: 'PATCH' });
    if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
    return res.json();
  },

  airports: {
    getAll:        ()      => API.get('/api/airports'),
    getByIata:     (iata)  => API.get(`/api/airports/iata/${iata}`),
    getTop10:      ()      => API.get('/api/airports/top10'),
    search:        (q)     => API.get(`/api/airports/search?q=${encodeURIComponent(q)}`),
    close:         (iata)  => API.patch(`/api/airports/iata/${iata}/close`),
    open:          (iata)  => API.patch(`/api/airports/iata/${iata}/open`),
  },

  flights: {
    getRoute:    (from, to) => API.get(`/api/flights/route?from=${from}&to=${to}`),
    getSchedule: (date, sortBy='departure') => API.get(`/api/flights/schedule?date=${date}&sortBy=${sortBy}`),
    getMST:      ()         => API.get('/api/flights/mst'),
    reroute:     (body)     => API.post('/api/flights/reroute', body),
    bfs:         (start)    => API.get(`/api/flights/bfs?start=${start}`),
    dfs:         (start)    => API.get(`/api/flights/dfs?start=${start}`),
    getByFN:     (fn)       => API.get(`/api/flights/number/${fn}`),
    search:      (q)        => API.get(`/api/flights/search?q=${encodeURIComponent(q)}`),
    byPrice:     (min, max) => API.get(`/api/flights/price?min=${min}&max=${max}`),
    getAll:      (page=1, pageSize=50, search='') => API.get(`/api/flights/all?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`),
    create:      (data)     => API.post('/api/flights', data),
    delete:      (fn)       => fetch(`/api/flights/${fn}`, { method: 'DELETE' }).then(r => r.json()),
    updateStatus:(fn, status) => fetch(`/api/flights/${fn}/status`, { method: 'PATCH', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ status }) }).then(r => r.json()),
  },

  passengers: {
    getByPNR:      (pnr)       => API.get(`/api/passengers/${pnr}`),
    getByPassport: (id)        => API.get(`/api/passengers/passport/${id}`),
    checkIn:       (data)      => API.post('/api/checkin', data),
    getQueue:      (flight)    => API.get(`/api/boarding/queue?flight=${flight}`),
    getBoardingGate: (flight) => API.get(`/api/boarding/gate?flight=${flight}`),
    boardNext:     (flight)    => API.post(`/api/boarding/next?flight=${flight}`, {}),
    loadCargo:     (item)      => API.post('/api/cargo/load', { item }),
    unloadCargo:   ()          => API.post('/api/cargo/unload', {}),
    getCargoStack: ()          => API.get('/api/cargo/stack'),
  },

  analytics: {
    getStats:      () => API.get('/api/analytics/stats'),
    getLeaderboard:() => API.get('/api/analytics/leaderboard'),
    benchmark:     () => API.get('/api/analytics/benchmark'),
  }
};

// ── Toast utility ─────────────────────────────────────────────────
function toast(msg, type = 'info') {
  const el = document.getElementById('toast');
  if (!el) return;
  el.textContent = msg;
  el.style.borderColor = type === 'error' ? '#ef4444' : type === 'success' ? '#22c55e' : '#2563eb';
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 3000);
}
