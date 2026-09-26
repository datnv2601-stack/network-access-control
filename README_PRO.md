# NAC Pro local setup

## 1. Install Node.js

- Download Node.js 18+
- Verify:

```bash
node -v
npm -v
```

## 2. Install packages

```bash
npm install
```

## 3. Start backend

```bash
node backend/src/server.js
```

## 4. Open frontend

Open `frontend/index.html` in a browser, or serve it using a local web server.

## 5. Test API

```bash
curl http://localhost:5000/api/health
```

## Notes

This is a local lab demo. It does not automatically change switch or firewall configuration. Real LAN blocking requires a controlled Linux gateway/firewall using iptables/nftables, or an approved Windows Firewall policy, with administrator privileges and a rollback plan. Do not run blocking rules on a production network without testing and a whitelist.
