// ── Flight number autocomplete (KMP search) ──────────────────────
let _fnTimer = null;
async function fnAC(inputId, dropId) {
  const q    = document.getElementById(inputId).value.trim().toUpperCase();
  const drop = document.getElementById(dropId);
  if (q.length < 2) { drop.style.display = 'none'; return; }
  clearTimeout(_fnTimer);
  _fnTimer = setTimeout(async () => {
    try {
      const results = await API.flights.search(q);
      if (!results || !results.length) { drop.style.display = 'none'; return; }
      drop.innerHTML = results.map(f => `
        <div class="fn-item" onmousedown="pickFN('${inputId}','${dropId}','${f.flightNumber}')">
          <span class="fn-num">${f.flightNumber}</span>
          <span class="fn-route">${f.originIata} → ${f.destinationIata}</span>
          <span style="margin-left:auto;color:#64748b;font-size:0.74rem">$${Math.round(f.price)}</span>
        </div>
      `).join('');
      drop.style.display = 'block';
    } catch(e) { drop.style.display = 'none'; }
  }, 200);
}

function pickFN(inputId, dropId, flightNumber) {
  document.getElementById(inputId).value = flightNumber;
  document.getElementById(dropId).style.display = 'none';
}

// Close dropdowns when clicking outside
document.addEventListener('click', e => {
  if (!e.target.closest('.fn-wrap')) {
    document.querySelectorAll('.fn-drop').forEach(d => d.style.display = 'none');
  }
});

// ── Flight search panel ───────────────────────────────────────────
let _fsTimer = null;
async function searchFlights() {
  const q    = document.getElementById('flightSearch').value.trim().toUpperCase();
  const drop = document.getElementById('flightSearchDrop');
  const res  = document.getElementById('flightSearchResult');
  if (q.length < 2) { drop.style.display = 'none'; res.innerHTML = ''; return; }
  clearTimeout(_fsTimer);
  _fsTimer = setTimeout(async () => {
    try {
      const flights = await API.flights.search(q);
      if (!flights || !flights.length) { drop.style.display = 'none'; res.innerHTML = '<div style="color:var(--muted);font-size:0.82rem">Reys topilmadi.</div>'; return; }
      drop.style.display = 'none';
      res.innerHTML = `
        <div style="font-size:0.75rem;color:var(--muted);margin-bottom:6px">${flights.length} ta reys topildi:</div>
        <div style="max-height:180px;overflow-y:auto">
          ${flights.map(f => `
            <div style="display:flex;align-items:center;gap:10px;padding:7px 10px;background:var(--bg3);border-radius:6px;margin-bottom:4px;cursor:pointer"
              onclick="selectFlight('${f.flightNumber}')">
              <span style="font-weight:700;color:#0ea5e9;min-width:60px">${f.flightNumber}</span>
              <span style="color:#94a3b8">${f.originIata} → ${f.destinationIata}</span>
              <span style="margin-left:auto;color:#22c55e;font-size:0.8rem">$${Math.round(f.price)}</span>
              <span style="font-size:0.72rem;color:var(--muted)">${f.status}</span>
            </div>
          `).join('')}
        </div>
      `;
    } catch(e) { res.innerHTML = `<div style="color:var(--red)">${e.message}</div>`; }
  }, 200);
}

function selectFlight(fn) {
  document.getElementById('flight').value = fn;
  document.getElementById('boardFlight').value = fn;
  document.getElementById('flightSearch').value = fn;
  document.getElementById('flightSearchResult').innerHTML =
    `<div style="color:var(--green);font-size:0.82rem">✅ ${fn} tanlandi — ro'yxat formasi to'ldirildi</div>`;
  toast(`${fn} tanlandi`, 'success');
}

// ── Check-in page logic ──────────────────────────────────────────
async function doCheckIn() {
  const fname  = document.getElementById('fname').value.trim();
  const lname  = document.getElementById('lname').value.trim();
  const passport = document.getElementById('passport').value.trim();
  const flight = document.getElementById('flight').value.trim().toUpperCase();
  const cls    = document.getElementById('cls').value;
  const nat    = document.getElementById('nationality').value.trim() || 'Unknown';

  if (!fname || !lname || !passport || !flight) {
    toast('Barcha majburiy maydonlarni to\'ldiring.', 'error'); return;
  }

  try {
    const res = await API.passengers.checkIn({
      firstName: fname, lastName: lname,
      passportId: passport, flightNumber: flight,
      ticketClass: cls, nationality: nat
    });
    toast(`✅ ${fname} ${lname} ro'yxatdan o'tdi! PNR: ${res.pnr}`, 'success');

    // Auto-refresh queue for this flight
    document.getElementById('boardFlight').value = flight;
    await loadQueue();
  } catch(e) { toast('Ro\'yxatdan o\'tishda xatolik: ' + e.message, 'error'); }
}

async function loadQueue() {
  const flight = document.getElementById('boardFlight').value.trim().toUpperCase();
  if (!flight) { toast('Reys raqamini kiriting', 'error'); return; }

  try {
    const queue = await API.passengers.getQueue(flight);
    const el    = document.getElementById('queueList');

    if (!queue.length) {
      el.innerHTML = '<div style="color:var(--muted);text-align:center;padding:20px">Navbat bo\'sh.</div>';
      return;
    }

    el.innerHTML = queue.map((p, i) => `
      <div class="queue-card ${p.ticketClass.toLowerCase()}">
        <div class="queue-pos">#${i+1}</div>
        <div>
          <div class="queue-name">${p.fullName}</div>
          <div style="font-size:0.72rem;color:var(--muted)">${p.pnr} · ${p.flightNumber}</div>
        </div>
        <div style="margin-left:auto">
          <span class="badge badge-${p.ticketClass.toLowerCase()}">${p.ticketClass}</span>
        </div>
      </div>
    `).join('');
  } catch(e) { toast('Navbat xatoligi: ' + e.message, 'error'); }
}

async function boardNext() {
  const flight = document.getElementById('boardFlight').value.trim().toUpperCase();
  if (!flight) { toast('Reys raqamini kiriting', 'error'); return; }
  try {
    const p = await API.passengers.boardNext(flight);
    if (p && p.fullName)
      toast(`🚶 ${p.fullName} [${p.ticketClass}] posadkaga o'tdi!`, 'success');
    await loadQueue();
  } catch(e) { toast(e.message, 'error'); }
}

async function loadCargo() {
  const item = document.getElementById('cargoItem').value.trim();
  if (!item) return;
  try {
    const r = await API.passengers.loadCargo(item);
    toast(`📦 '${item}' yuklandi. Stek: ${r.stackSize} ta element`, 'success');
    document.getElementById('cargoItem').value = '';
    await refreshCargo();
  } catch(e) { toast(e.message, 'error'); }
}

async function unloadCargo() {
  try {
    const r = await API.passengers.unloadCargo();
    if (r.message) toast(r.message, 'info');
    else toast(`📦 '${r.unloaded}' yuk bo'limidan tushirildi`, 'success');
    await refreshCargo();
  } catch(e) { toast(e.message, 'error'); }
}

async function refreshCargo() {
  try {
    const r  = await API.passengers.getCargoStack();
    const el = document.getElementById('cargoStack');
    if (!r.items.length) {
      el.innerHTML = '<div style="color:var(--muted);font-size:0.8rem">Stek bo\'sh.</div>'; return;
    }
    el.innerHTML = r.items.map((item, i) => `
      <div class="cargo-item">
        <div class="cargo-pos">${i === 0 ? '⬆ TEPA' : i + 1}</div>
        <div>${item}</div>
      </div>
    `).join('');
  } catch(e) { toast(e.message, 'error'); }
}

// Real-time SignalR
function onQueueUpdated(data) {
  const curr = document.getElementById('boardFlight').value.trim().toUpperCase();
  if (curr === data.flight) loadQueue();
}
