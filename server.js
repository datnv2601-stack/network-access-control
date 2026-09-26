const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

const policies = [
  {
    id: 1,
    name: 'Office-Staff',
    type: 'allow',
    condition: '192.168.10.0/24',
    action: 'allow',
    description: 'Cho phép các máy văn phòng truy cập nội bộ',
  },
  {
    id: 2,
    name: 'Guest-WiFi',
    type: 'guest',
    condition: '192.168.50.0/24',
    action: 'restricted',
    description: 'Khách truy cập internet nhưng bị hạn chế nội bộ',
  },
  {
    id: 3,
    name: 'IoT-Quarantine',
    type: 'block',
    condition: '00:1A:2B:*',
    action: 'quarantine',
    description: 'Thiết bị IoT cần được kiểm tra trước khi cấp quyền',
  },
];

const devices = [
  {
    id: 1,
    name: 'PC-01',
    ip: '192.168.10.12',
    mac: '00:1A:2B:3C:4D:5E',
    vlan: 'VLAN 10',
    role: 'staff',
    status: 'approved',
    lastSeen: '2026-09-26T09:15:00Z',
    notes: 'Máy làm việc chính của phòng kỹ thuật',
  },
  {
    id: 2,
    name: 'Laptop-Dev',
    ip: '192.168.10.18',
    mac: 'AA:BB:CC:DD:EE:FF',
    vlan: 'VLAN 10',
    role: 'developer',
    status: 'pending',
    lastSeen: '2026-09-26T08:47:00Z',
    notes: 'Cần xác thực thêm trước khi cấp quyền',
  },
  {
    id: 3,
    name: 'Printer-01',
    ip: '192.168.20.44',
    mac: '10:20:30:40:50:60',
    vlan: 'VLAN 20',
    role: 'printer',
    status: 'blocked',
    lastSeen: '2026-09-26T07:40:00Z',
    notes: 'Thiết bị không thuộc danh sách approved',
  },
  {
    id: 4,
    name: 'Guest-Phone',
    ip: '192.168.50.33',
    mac: '98:AA:BC:12:45:67',
    vlan: 'VLAN 50',
    role: 'guest',
    status: 'quarantined',
    lastSeen: '2026-09-26T09:00:00Z',
    notes: 'Khách truy cập giới hạn theo chính sách Guest WiFi',
  },
];

function createSummary() {
  return {
    total: devices.length,
    approved: devices.filter((d) => d.status === 'approved').length,
    pending: devices.filter((d) => d.status === 'pending').length,
    blocked: devices.filter((d) => d.status === 'blocked').length,
    quarantined: devices.filter((d) => d.status === 'quarantined').length,
  };
}

app.get('/api/summary', (req, res) => {
  res.json(createSummary());
});

app.get('/api/devices', (req, res) => {
  res.json(devices);
});

app.get('/api/policies', (req, res) => {
  res.json(policies);
});

app.post('/api/devices', (req, res) => {
  const { name, ip, mac, vlan, role, status = 'pending', notes = '' } = req.body;

  if (!name || !ip || !mac) {
    return res.status(400).json({ message: 'name, ip và mac là bắt buộc.' });
  }

  const newDevice = {
    id: Date.now(),
    name,
    ip,
    mac,
    vlan: vlan || 'VLAN 10',
    role: role || 'unknown',
    status,
    lastSeen: new Date().toISOString(),
    notes,
  };

  devices.unshift(newDevice);
  res.status(201).json(newDevice);
});

app.patch('/api/devices/:id', (req, res) => {
  const { id } = req.params;
  const device = devices.find((d) => String(d.id) === id);

  if (!device) {
    return res.status(404).json({ message: 'Không tìm thấy thiết bị.' });
  }

  const { status, notes, vlan, role, ip, mac } = req.body;

  if (status) device.status = status;
  if (notes) device.notes = notes;
  if (vlan) device.vlan = vlan;
  if (role) device.role = role;
  if (ip) device.ip = ip;
  if (mac) device.mac = mac;

  device.lastSeen = new Date().toISOString();

  res.json(device);
});

app.post('/api/policies', (req, res) => {
  const { name, type, condition, action, description } = req.body;

  if (!name || !type || !condition || !action) {
    return res.status(400).json({ message: 'Vui lòng gửi đủ các trường: name, type, condition, action.' });
  }

  const newPolicy = {
    id: Date.now(),
    name,
    type,
    condition,
    action,
    description: description || '',
  };

  policies.push(newPolicy);
  res.status(201).json(newPolicy);
});

app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

app.listen(PORT, () => {
  console.log(`NAC demo is running on http://localhost:${PORT}`);
});
