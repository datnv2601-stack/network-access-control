const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const fs = require('fs');
const path = require('path');
const { exec } = require('child_process');
const os = require('os');

const app = express();
const PORT = 5000;
const DATA_DIR = path.join(__dirname, 'data');
const DATA_FILE = path.join(DATA_DIR, 'devices.json');
const BLOCKED_FILE = path.join(DATA_DIR, 'blocked.json');

app.use(cors());
app.use(bodyParser.json());
app.use(express.static(path.join(__dirname, 'public')));

// Ensure data directory
if (!fs.existsSync(DATA_DIR)) {
  fs.mkdirSync(DATA_DIR, { recursive: true });
}

function readDevices() {
  if (!fs.existsSync(DATA_FILE)) {
    return [];
  }
  return JSON.parse(fs.readFileSync(DATA_FILE, 'utf8'));
}

function writeDevices(devices) {
  fs.writeFileSync(DATA_FILE, JSON.stringify(devices, null, 2));
}

function readBlocked() {
  if (!fs.existsSync(BLOCKED_FILE)) {
    return [];
  }
  return JSON.parse(fs.readFileSync(BLOCKED_FILE, 'utf8'));
}

function writeBlocked(blocked) {
  fs.writeFileSync(BLOCKED_FILE, JSON.stringify(blocked, null, 2));
}

function applyBlockRule(ip, mac) {
  const isWindows = os.platform() === 'win32';
  
  if (isWindows) {
    // Windows Firewall
    const ruleName = `NAC-Block-${mac.replace(/:/g, '-')}`;
    const cmd = `powershell -Command "New-NetFirewallRule -DisplayName '${ruleName}' -RemoteAddress ${ip} -Direction Inbound -Action Block 2>$null"`;
    exec(cmd, (err) => {
      if (!err) {
        console.log(`✓ Blocked ${ip} (${mac}) with Windows Firewall`);
      }
    });
  } else {
    // Linux iptables
    const cmds = [
      `sudo iptables -I INPUT -s ${ip} -j DROP`,
      `sudo iptables -I INPUT -m mac --mac-source ${mac} -j DROP`
    ];
    cmds.forEach(cmd => {
      exec(cmd, (err) => {
        if (!err) {
          console.log(`✓ Applied iptables rule: ${cmd}`);
        }
      });
    });
  }
}

function removeBlockRule(ip, mac) {
  const isWindows = os.platform() === 'win32';
  
  if (isWindows) {
    const ruleName = `NAC-Block-${mac.replace(/:/g, '-')}`;
    const cmd = `powershell -Command "Remove-NetFirewallRule -DisplayName '${ruleName}' 2>$null"`;
    exec(cmd, (err) => {
      if (!err) {
        console.log(`✓ Unblocked ${ip} (${mac})`);
      }
    });
  } else {
    const cmds = [
      `sudo iptables -D INPUT -s ${ip} -j DROP`,
      `sudo iptables -D INPUT -m mac --mac-source ${mac} -j DROP`
    ];
    cmds.forEach(cmd => {
      exec(cmd, (err) => {
        if (!err) {
          console.log(`✓ Removed iptables rule`);
        }
      });
    });
  }
}

// Health check
app.get('/api/health', (req, res) => {
  res.json({
    ok: true,
    service: 'nac-pro',
    timestamp: new Date().toISOString(),
    platform: os.platform()
  });
});

// Get all devices
app.get('/api/devices', (req, res) => {
  const devices = readDevices();
  const blocked = readBlocked();
  const summary = {
    total: devices.length,
    approved: devices.filter(d => d.status === 'approved').length,
    blocked: devices.filter(d => d.status === 'blocked').length,
    unknown: devices.filter(d => d.status === 'unknown').length,
    pending: devices.filter(d => d.status === 'pending').length,
    blockedRules: blocked.length
  };
  
  res.json({ devices, summary, blocked });
});

// Add/Update device via scanner
app.post('/api/devices/add', (req, res) => {
  const { ip, mac, name, vendor } = req.body;
  
  if (!ip || !mac) {
    return res.status(400).json({ error: 'ip and mac required' });
  }

  const devices = readDevices();
  const existing = devices.find(d => d.mac.toLowerCase() === mac.toLowerCase());
  
  if (existing) {
    existing.ip = ip;
    existing.name = name || existing.name;
    existing.vendor = vendor || existing.vendor;
    existing.lastSeen = new Date().toISOString();
  } else {
    devices.unshift({
      id: Date.now(),
      name: name || 'Unknown',
      ip,
      mac: mac.toUpperCase(),
      vendor: vendor || 'Unknown',
      status: 'unknown',
      firstSeen: new Date().toISOString(),
      lastSeen: new Date().toISOString()
    });
  }
  
  writeDevices(devices);
  res.json({ ok: true });
});

// Update device status
app.patch('/api/devices/:id', (req, res) => {
  const { status } = req.body;
  const devices = readDevices();
  const device = devices.find(d => d.id == req.params.id);
  
  if (!device) {
    return res.status(404).json({ error: 'Device not found' });
  }
  
  const oldStatus = device.status;
  device.status = status || device.status;
  device.lastUpdated = new Date().toISOString();
  
  // Apply firewall rules
  if (status === 'blocked' && oldStatus !== 'blocked') {
    const blocked = readBlocked();
    if (!blocked.find(b => b.ip === device.ip)) {
      blocked.unshift({
        id: Date.now(),
        ip: device.ip,
        mac: device.mac,
        name: device.name,
        reason: 'Manual block',
        blockedAt: new Date().toISOString()
      });
      writeBlocked(blocked);
      applyBlockRule(device.ip, device.mac);
    }
  } else if (status === 'approved' && oldStatus === 'blocked') {
    const blocked = readBlocked();
    const idx = blocked.findIndex(b => b.ip === device.ip || b.mac === device.mac);
    if (idx >= 0) {
      removeBlockRule(blocked[idx].ip, blocked[idx].mac);
      blocked.splice(idx, 1);
      writeBlocked(blocked);
    }
  }
  
  writeDevices(devices);
  res.json(device);
});

// Delete device
app.delete('/api/devices/:id', (req, res) => {
  const devices = readDevices();
  const idx = devices.findIndex(d => d.id == req.params.id);
  
  if (idx < 0) {
    return res.status(404).json({ error: 'Device not found' });
  }
  
  const device = devices[idx];
  
  // Remove block rule if exists
  const blocked = readBlocked();
  const bidx = blocked.findIndex(b => b.ip === device.ip);
  if (bidx >= 0) {
    removeBlockRule(blocked[bidx].ip, blocked[bidx].mac);
    blocked.splice(bidx, 1);
    writeBlocked(blocked);
  }
  
  devices.splice(idx, 1);
  writeDevices(devices);
  
  res.status(204).send();
});

// Get blocked devices
app.get('/api/blocked', (req, res) => {
  const blocked = readBlocked();
  res.json(blocked);
});

// Manually block IP/MAC
app.post('/api/blocked', (req, res) => {
  const { ip, mac, name, reason } = req.body;
  
  if (!ip && !mac) {
    return res.status(400).json({ error: 'ip or mac required' });
  }
  
  const blocked = readBlocked();
  if (blocked.find(b => b.ip === ip || b.mac === mac)) {
    return res.status(409).json({ error: 'Already blocked' });
  }
  
  const entry = {
    id: Date.now(),
    ip: ip || '',
    mac: (mac || '').toUpperCase(),
    name: name || 'Unknown',
    reason: reason || 'Manual',
    blockedAt: new Date().toISOString()
  };
  
  blocked.unshift(entry);
  writeBlocked(blocked);
  
  if (ip && mac) {
    applyBlockRule(ip, mac);
  }
  
  res.status(201).json(entry);
});

// Remove from blocked
app.delete('/api/blocked/:id', (req, res) => {
  const blocked = readBlocked();
  const idx = blocked.findIndex(b => b.id == req.params.id);
  
  if (idx < 0) {
    return res.status(404).json({ error: 'Not found' });
  }
  
  const entry = blocked[idx];
  removeBlockRule(entry.ip, entry.mac);
  
  blocked.splice(idx, 1);
  writeBlocked(blocked);
  
  res.status(204).send();
});

// Clear all blocks (emergency reset)
app.post('/api/firewall/reset', (req, res) => {
  console.log('⚠️  FIREWALL RESET TRIGGERED');
  
  const isWindows = os.platform() === 'win32';
  const blocked = readBlocked();
  
  blocked.forEach(entry => {
    removeBlockRule(entry.ip, entry.mac);
  });
  
  writeBlocked([]);
  
  res.json({ ok: true, message: 'All firewall rules cleared' });
});

app.listen(PORT, () => {
  console.log(`🚀 NAC Pro backend running at http://localhost:${PORT}`);
  console.log(`📊 Dashboard: http://localhost:${PORT}`);
  console.log(`📡 Platform: ${os.platform()}`);
});
