// ── SkyNet SignalR Client ─────────────────────────────────────────
let skyNetConnection = null;

async function initSignalR() {
  skyNetConnection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/skynet')
    .withAutomaticReconnect()
    .build();

  skyNetConnection.on('Connected', (data) => {
    console.log('SkyNet SignalR connected:', data.connectionId);
    updateConnectionStatus(true);
  });

  skyNetConnection.on('QueueUpdated', (data) => {
    toast(`Queue updated: Flight ${data.flight} — ${data.queueSize} passengers`, 'info');
    if (typeof onQueueUpdated === 'function') onQueueUpdated(data);
  });

  skyNetConnection.on('FlightStatusChanged', (data) => {
    toast(`Flight ${data.flightNumber}: ${data.status}`, 'info');
    if (typeof onFlightStatusChanged === 'function') onFlightStatusChanged(data);
  });

  skyNetConnection.on('EmergencyAlert', (data) => {
    toast(`🚨 EMERGENCY: Airport ${data.airport} — ${data.routes || 0} alt routes`, 'error');
    if (typeof onEmergencyAlert === 'function') onEmergencyAlert(data);
  });

  skyNetConnection.on('PassengerBoarded', (data) => {
    toast(`✈ ${data.name} [${data.cls}] boarded ${data.flight}`, 'success');
    if (typeof onPassengerBoarded === 'function') onPassengerBoarded(data);
  });

  skyNetConnection.onreconnecting(() => updateConnectionStatus(false));
  skyNetConnection.onreconnected(() => updateConnectionStatus(true));

  try {
    await skyNetConnection.start();
  } catch (err) {
    console.warn('SignalR connection failed:', err);
    updateConnectionStatus(false);
  }
}

function updateConnectionStatus(connected) {
  const dot = document.querySelector('.status-dot');
  if (!dot) return;
  dot.style.background = connected ? 'var(--green)' : 'var(--red)';
  dot.style.boxShadow  = connected ? '0 0 6px var(--green)' : '0 0 6px var(--red)';
}

document.addEventListener('DOMContentLoaded', () => {
  if (typeof signalR !== 'undefined') initSignalR();
});
