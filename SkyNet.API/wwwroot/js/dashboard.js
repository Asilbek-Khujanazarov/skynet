// ── Dashboard logic ──────────────────────────────────────────────
let priceChart = null;

async function loadStats() {
  try {
    const s = await API.analytics.getStats();
    document.getElementById('s-airports').textContent   = s.totalAirports.toLocaleString();
    document.getElementById('s-flights').textContent    = s.totalFlights.toLocaleString();
    document.getElementById('s-passengers').textContent = s.totalPassengers.toLocaleString();
    document.getElementById('s-checkedin').textContent  = s.checkedIn.toLocaleString();
    document.getElementById('s-active').textContent     = s.activeFlights.toLocaleString();
    document.getElementById('s-price').textContent      = '$' + Math.round(s.avgFlightPrice);
  } catch(e) { toast('Statistika xatoligi: ' + e.message, 'error'); }
}

async function loadLeaderboard() {
  try {
    const airports = await API.analytics.getLeaderboard();
    const max = airports[0]?.usageCount || 1;
    document.getElementById('leaderboard').innerHTML = airports.map((a, i) => `
      <div class="leaderboard-row">
        <div class="rank rank-${i+1}">${i+1}</div>
        <div style="width:44px;font-weight:700;color:var(--accent2)">${a.iataCode}</div>
        <div style="flex:1;min-width:0">
          <div style="font-size:0.78rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${a.name}</div>
          <div class="bar-wrap" style="margin-top:3px">
            <div class="bar-fill" style="width:${Math.round((a.usageCount/max)*100)}%"></div>
          </div>
        </div>
        <div style="font-size:0.75rem;color:var(--muted);width:54px;text-align:right">${a.usageCount.toLocaleString()}</div>
      </div>
    `).join('');
  } catch(e) { toast('Reytingni yuklashda xatolik: ' + e.message, 'error'); }
}

async function loadPriceChart() {
  try {
    const flights = await API.flights.byPrice(0, 2000);
    const buckets = [0,100,200,300,400,500,700,1000,2000];
    const labels  = buckets.slice(0,-1).map((b,i) => `$${b}–$${buckets[i+1]}`);
    const counts  = new Array(labels.length).fill(0);

    flights.forEach(f => {
      for (let i = 0; i < buckets.length - 1; i++) {
        if (f.price >= buckets[i] && f.price < buckets[i+1]) { counts[i]++; break; }
      }
    });

    const ctx = document.getElementById('priceChart').getContext('2d');
    if (priceChart) priceChart.destroy();
    priceChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [{ label: 'Flights', data: counts,
          backgroundColor: 'rgba(37,99,235,0.6)', borderColor: '#2563eb',
          borderWidth: 1, borderRadius: 4 }]
      },
      options: {
        responsive: true, plugins: { legend: { display: false } },
        scales: {
          x: { ticks: { color: '#64748b', font: { size: 10 } }, grid: { color: 'rgba(255,255,255,0.05)' } },
          y: { ticks: { color: '#64748b' }, grid: { color: 'rgba(255,255,255,0.05)' } }
        }
      }
    });
  } catch(e) { toast('Grafik xatoligi: ' + e.message, 'error'); }
}

async function runBenchmark() {
  const el = document.getElementById('benchmarkResult');
  el.innerHTML = '<div style="color:var(--muted)">Sinov ishlayapti...</div>';
  try {
    const r = await API.analytics.benchmark();
    const qs = r.quickSortMs, ms = r.mergeSortMs;
    el.innerHTML = `
      <div class="bench-row">
        <span>Ma'lumotlar hajmi</span><span class="bench-val">${r.dataSize.toLocaleString()} ta yozuv</span>
      </div>
      <div class="bench-row">
        <span>⚡ QuickSort</span>
        <span class="bench-val" style="color:${qs <= ms ? 'var(--green)' : 'var(--red)'}">${qs} ms</span>
      </div>
      <div class="bench-row">
        <span>🔀 MergeSort</span>
        <span class="bench-val" style="color:${ms <= qs ? 'var(--green)' : 'var(--red)'}">${ms} ms</span>
      </div>
      <div style="margin-top:10px;padding:8px;background:rgba(34,197,94,0.1);border-radius:6px;font-size:0.82rem">
        🏆 G'olib: <b>${r.fasterAlgorithm}</b>
      </div>
      <button class="btn btn-ghost" style="margin-top:8px;width:100%" onclick="runBenchmark()">Qayta ishlatish</button>
    `;
  } catch(e) { toast('Sinov xatoligi: ' + e.message, 'error'); }
}

async function loadSchedule() {
  const sortBy = document.getElementById('sortBy').value;
  const today  = new Date().toISOString().split('T')[0];
  try {
    const flights = await API.flights.getSchedule(today, sortBy);
    const tbody   = document.getElementById('scheduleBody');
    tbody.innerHTML = flights.slice(0, 30).map(f => `
      <tr>
        <td><b>${f.flightNumber}</b></td>
        <td>${f.originIata} → ${f.destinationIata}</td>
        <td>${new Date(f.departureTime).toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'})}</td>
        <td>$${Math.round(f.price)}</td>
        <td><span class="badge badge-active">${f.status}</span></td>
      </tr>
    `).join('');
  } catch(e) { toast('Jadval xatoligi: ' + e.message, 'error'); }
}

function refresh() {
  loadStats();
  loadLeaderboard();
  loadPriceChart();
}

document.addEventListener('DOMContentLoaded', () => {
  refresh();
  loadSchedule();
  // Auto-refresh every 30s
  setInterval(refresh, 30000);
});
