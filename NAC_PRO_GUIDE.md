# NAC Pro - Full Network Access Control System

**Hệ thống quét ARP, phát hiện thiết bị, chặn IP/MAC tự động**

## 🎯 Tính năng

✅ **Quét ARP tự động** - Phát hiện thiết bị trong LAN mỗi 30 giây
✅ **Chặn IP/MAC** - Sử dụng iptables (Linux) hoặc Windows Firewall
✅ **Dashboard realtime** - Theo dõi thiết bị, approve/block tức thì
✅ **Firewall rules tự động** - Khi chặn thiết bị sẽ apply rule ngay
✅ **Persistent storage** - Lưu trữ trong PostgreSQL
✅ **Audit logging** - Ghi nhật ký mọi thao tác
✅ **API REST** - Dễ tích hợp với hệ thống khác

## 📋 Yêu cầu

- **Linux** (Ubuntu 20.04+) hoặc **Windows 10+**
- Docker & Docker Compose
- Python 3.8+
- Node.js 18+
- PostgreSQL 14+
- Quyền root/admin

## 📁 Cấu trúc project

```
nac-pro/
├── docker-compose.yml
├── .env.example
├── backend/
│   ├── src/
│   │   ├── server.js
│   │   ├── config/
│   │   ├── routes/
│   │   ├── controllers/
│   │   ├── models/
│   │   ├── services/
│   │   └── middleware/
│   ├── package.json
│   └── Dockerfile
├── scanner/
│   ├── arp_scanner.py
│   ├── firewall_manager.py
│   ├── requirements.txt
│   └── Dockerfile
├── frontend/
│   ├── src/
│   ├── package.json
│   ├── public/
│   └── Dockerfile
└── docs/
    └── SETUP.md
```

## 🚀 Cài đặt nhanh

### Cách 1: Chạy với Docker Compose (Khuyến dùng)

```bash
cd nac-pro
cp .env.example .env
# Chỉnh sửa .env nếu cần

docker-compose up -d
```

Sau 30 giây:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- PostgreSQL: localhost:5432

### Cách 2: Chạy trên Windows (Local)

```bash
cd backend
npm install
node src/server.js

# Mở terminal khác
cd scanner
pip install -r requirements.txt
python arp_scanner.py
```

### Cách 3: Chạy trên Linux (Local)

```bash
cd backend
npm install
sudo node src/server.js  # Cần quyền root để iptables

cd scanner
pip install -r requirements.txt
sudo python3 arp_scanner.py  # Cần quyền root
```

## 📖 Hướng dẫn sử dụng

### 1. Mở dashboard

```text
http://localhost:3000
```

### 2. Scanner tự động quét

Cứ 30 giây, hệ thống sẽ quét ARP và cập nhật danh sách thiết bị

### 3. Approve/Block thiết bị

- **Approve**: Thiết bị được phép truy cập
- **Block**: Chặn IP và MAC (không thể traffic)
- **Quarantine**: Cách ly, chỉ cho phép kết nối NAC server

### 4. Khi chặn thiết bị

**Trên Linux (iptables)**:
```bash
iptables -I INPUT -s 192.168.1.10 -j DROP
iptables -I INPUT -m mac --mac-source AA:BB:CC:DD:EE:FF -j DROP
```

**Trên Windows (Firewall)**:
```powershell
New-NetFirewallRule -DisplayName "Block-IP" -RemoteAddress 192.168.1.10 -Direction Inbound -Action Block
```

## 🔌 API Endpoints

### Devices
```bash
GET /api/devices              # Danh sách thiết bị
GET /api/devices/:id          # Chi tiết thiết bị
POST /api/devices             # Tạo thiết bị
PATCH /api/devices/:id        # Cập nhật status
DELETE /api/devices/:id       # Xóa thiết bị
```

### Firewall Rules
```bash
GET /api/firewall/rules       # Danh sách rule
POST /api/firewall/rules      # Tạo rule
DELETE /api/firewall/rules/:id # Xóa rule
POST /api/firewall/apply      # Apply ngay
```

### Scanner
```bash
GET /api/scanner/status       # Trạng thái scanner
POST /api/scanner/start       # Bắt đầu quét
POST /api/scanner/stop        # Dừng quét
```

## 🔒 Bảo mật

⚠️ **LƯU Ý**: Phiên bản này chưa có xác thực. Cần thêm:

- JWT authentication
- HTTPS/TLS
- Rate limiting
- RBAC (Admin, Operator, Viewer)
- Audit logging

## ⚠️ Lưu ý quan trọng

1. **Chạy từ từ** - Không chặn tất cả thiết bị ngay, vì có thể làm mất kết nối
2. **Whitelist quan trọng** - Luôn keep gateway, DNS, NAC server
3. **Test trước** - Test chặn một thiết bị, kiểm tra xem có hiệu quả không
4. **Backup rules** - Lưu iptables backup trước khi chạy
5. **Có nút rollback** - Chuẩn bị cách khôi phục nhanh

## 🛠️ Troubleshooting

### Scanner không quét được
```bash
sudo python3 arp_scanner.py  # Cần quyền root
```

### Firewall rule không áp dụng
```bash
sudo iptables -L -n  # Xem rule
sudo systemctl restart docker  # Restart
```

### Database connection error
```bash
docker-compose logs postgres
```

## 📚 Tài liệu

Xem chi tiết tại [docs/SETUP.md](./docs/SETUP.md)

---

**⚡ Ready to deploy? Go to next section!**
