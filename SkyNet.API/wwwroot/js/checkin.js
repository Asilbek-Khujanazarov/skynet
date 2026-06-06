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
