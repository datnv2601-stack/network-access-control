const express = require('express');
const path = require('path');
const fs = require('fs');

const app = express();
const PORT = Number(process.env.PORT || 3000);
const DATA_DIR = path.join(__dirname, 'data');
const DATA_FILE = path.join(DATA_DIR, 'store.json');

app.use(express.json({ limit: '1mb' }));
app.use(express.static(path.join(__dirname, 'public')));

const initialData = {
  devices: [
    { id: 1, name: 'PC-01', ip: '192.168.10.12', mac: '00:1A:2B:3C:4D:5E', vlan: 10, role: 'staff', status: 'approved', lastSeen: new Date().toISOString(), notes: 'Máy phòng kỹ thuật' },
    { id: 2, name: 'Laptop-Dev', ip: '192.168.10.18', mac: 'AA:BB:CC:DD:EE:FF', vlan: 10, role: 'developer', status: 'pending', lastSeen: new Date().toISOString(), notes: 'Chờ phê duyệt' },
    { id: 3, name: 'Printer-01', ip: '192.168.20.44', mac: '10:20:30:40:50:60', vlan: 20, role: 'printer', status: 'blocked', lastSeen: new Date().toISOString(), notes: 'Thiết bị bị chặn' },
    { id: 4, name: 'Guest-Phone', ip: '192.168.50.33', mac: '98:AA:BC:12:45:67', vlan: 50, role: 'guest', status: 'quarantined', lastSeen: new Date().toISOString(), notes: 'Truy cập giới hạn' }
  ],
  policies: [
    { id: 1, name: 'Office-Staff', type: 'allow', condition: '192.168.10.0/24', action: 'allow', description: 'Cho phép mạng nhân viên' },
    { id: 2, name: 'Guest-WiFi', type: 'restrict', condition: '192.168.50.0/24', action: 'restricted', description: 'Chỉ cho phép Internet' },
    { id: 3, name: 'IoT-Quarantine', type: 'block', condition: '00:1A:2B:*', action: 'quarantine', description: 'Cách ly thiết bị chưa xác minh' }
  ],
  events: []
};

function ensureStore() {
  fs.mkdirSync(DATA_DIR, { recursive: true });
  if (!fs.existsSync(DATA_FILE)) fs.writeFileSync(DATA_FILE, JSON.stringify(initialData, null, 2));
}
function readStore() { ensureStore(); return JSON.parse(fs.readFileSync(DATA_FILE, 'utf8')); }
function writeStore(data) { fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2)); }
function event(store, message, severity = 'info') {
  store.events.unshift({ id: Date.now(), message, severity, createdAt: new Date().toISOString() });
  store.events = store.events.slice(0, 100);
}
function validIp(ip) { return /^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$/.test(ip); }
function validMac(mac) { return /^([0-9A-F]{2}:){5}[0-9A-F]{2}$/i.test(mac); }
function summary(devices) {
  return { total: devices.length, approved: devices.filter(x => x.status === 'approved').length, pending: devices.filter(x => x.status === 'pending').length, blocked: devices.filter(x => x.status === 'blocked').length, quarantined: devices.filter(x => x.status === 'quarantined').length };
}

app.get('/api/health', (req, res) => res.json({ ok: true, service: 'nac-demo', time: new Date().toISOString() }));
app.get('/api/summary', (req, res) => res.json(summary(readStore().devices)));
app.get('/api/devices', (req, res) => {
  const store = readStore();
  const q = String(req.query.q || '').toLowerCase();
  const status = String(req.query.status || '');
  let result = store.devices;
  if (q) result = result.filter(d => [d.name, d.ip, d.mac, d.role].some(v => String(v).toLowerCase().includes(q)));
  if (status) result = result.filter(d => d.status === status);
  res.json(result);
});
app.get('/api/policies', (req, res) => res.json(readStore().policies));
app.get('/api/events', (req, res) => res.json(readStore().events));

app.post('/api/devices', (req, res) => {
  const { name, ip, mac, vlan = 10, role = 'unknown', status = 'pending', notes = '' } = req.body || {};
  if (!name || !validIp(ip) || !validMac(mac)) return res.status(400).json({ message: 'Tên, IPv4 và MAC hợp lệ là bắt buộc.' });
  if (!['approved', 'pending', 'blocked', 'quarantined'].includes(status)) return res.status(400).json({ message: 'Trạng thái không hợp lệ.' });
  const store = readStore();
  if (store.devices.some(d => d.mac.toLowerCase() === mac.toLowerCase())) return res.status(409).json({ message: 'MAC đã tồn tại.' });
  const device = { id: Date.now(), name, ip, mac: mac.toUpperCase(), vlan: Number(vlan), role, status, lastSeen: new Date().toISOString(), notes };
  store.devices.unshift(device); event(store, `Đã thêm thiết bị ${name}`, 'info'); writeStore(store); res.status(201).json(device);
});

app.patch('/api/devices/:id', (req, res) => {
  const store = readStore();
  const device = store.devices.find(d => String(d.id) === req.params.id);
  if (!device) return res.status(404).json({ message: 'Không tìm thấy thiết bị.' });
  const allowed = ['approved', 'pending', 'blocked', 'quarantined'];
  if (req.body.status && !allowed.includes(req.body.status)) return res.status(400).json({ message: 'Trạng thái không hợp lệ.' });
  Object.assign(device, { ...req.body, lastSeen: new Date().toISOString() });
  event(store, `Thiết bị ${device.name} chuyển sang ${device.status}`, device.status === 'approved' ? 'success' : 'warning');
  writeStore(store); res.json(device);
});

app.delete('/api/devices/:id', (req, res) => {
  const store = readStore(); const index = store.devices.findIndex(d => String(d.id) === req.params.id);
  if (index < 0) return res.status(404).json({ message: 'Không tìm thấy thiết bị.' });
  const [removed] = store.devices.splice(index, 1); event(store, `Đã xóa thiết bị ${removed.name}`, 'warning'); writeStore(store); res.status(204).end();
});

app.post('/api/policies', (req, res) => {
  const { name, type, condition, action, description = '' } = req.body || {};
  if (!name || !type || !condition || !action) return res.status(400).json({ message: 'Vui lòng nhập đ��� thông tin policy.' });
  const store = readStore(); const policy = { id: Date.now(), name, type, condition, action, description };
  store.policies.push(policy); event(store, `Đã tạo policy ${name}`, 'info'); writeStore(store); res.status(201).json(policy);
});

app.delete('/api/policies/:id', (req, res) => {
  const store = readStore(); const index = store.policies.findIndex(p => String(p.id) === req.params.id);
  if (index < 0) return res.status(404).json({ message: 'Không tìm thấy policy.' });
  store.policies.splice(index, 1); writeStore(store); res.status(204).end();
});

app.get('*', (req, res) => res.sendFile(path.join(__dirname, 'public', 'index.html')));
app.listen(PORT, () => console.log(`NAC demo running at http://localhost:${PORT}`));
