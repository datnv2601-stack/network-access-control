# NAC Pro - Complete LAN Device Controller

## Tính năng

✅ Quét ARP/DHCP phát hiện thiết bị  
✅ Chặn IP/MAC bằng iptables (Linux) hoặc Windows Firewall  
✅ Dashboard realtime  
✅ Approve/Block/Delete thiết bị  
✅ Blocked list quản lý  
✅ Events logging  

## Cài đặt & Chạy

### 1. Cài Node.js (nếu chưa có)

Tải từ: https://nodejs.org/ (LTS version)

### 2. Cài dependencies

```bash
npm install
```

### 3. Chạy backend

```bash
node server.js
```

Hoặc chế độ dev (tự reload):

```bash
npm run dev
```

### 4. Mở dashboard

Mở trình duyệt:

```
http://localhost:5000
```

## Cách sử dụng

1. **Quét mạng**: Click "📡 Quét mạng (Demo)" để thêm thiết bị mẫu
2. **Thêm thiết bị**: Nhập thủ công IP, MAC, tên
3. **Approve**: Cho phép thiết bị truy cập
4. **Block**: Chặn thiết bị (sẽ áp dụng firewall rule nếu có quyền)
5. **Unblock**: Gỡ bỏ quy tắc chặn

## Firewall Rules (Linux)

Trên Linux, khi chặn thiết bị hệ thống sẽ chạy:

```bash
sudo iptables -I INPUT -s 192.168.1.10 -j DROP          # Chặn IP
sudo iptables -I INPUT -m mac --mac-source AA:BB:CC:DD:EE:FF -j DROP  # Chặn MAC
```

Để chạy với iptables, khởi động với sudo:

```bash
sudo node server.js
```

## Windows Firewall

Trên Windows, sẽ tạo rule tự động qua PowerShell (cần chạy quyền Admin):

```powershell
New-NetFirewallRule -DisplayName "NAC-Block-..." -RemoteAddress 192.168.1.10 -Direction Inbound -Action Block
```

## API Endpoints

```bash
GET  /api/devices              # Danh sách + summary
GET  /api/blocked              # Danh sách chặn
POST /api/devices/add          # Thêm thiết bị
PATCH /api/devices/:id         # Update status
DELETE /api/devices/:id        # Xóa thiết bị
POST /api/blocked              # Chặn IP/MAC
DELETE /api/blocked/:id        # Unblock
POST /api/firewall/reset       # Gỡ tất cả rules
```

## Lưu ý an toàn

⚠️ **Đây là bản LAB chỉ để học và test**

- Không nên chạy trên mạng production mà chưa cấu hình đầy đủ
- Luôn có whitelist các thiết bị quan trọng (gateway, DNS, NAC server)
- Test chặn 1 thiết bị trước, kiểm tra hiệu quả
- Giữ quyền admin/sudo để gỡ rules nhanh nếu cần
- Firewall rules được áp dụng trực tiếp - cần cẩn thận

## Troubleshooting

**Firewall rule không apply?**
- Trên Linux: Chạy với `sudo node server.js`
- Trên Windows: Chạy terminal as Administrator

**Không thấy thiết bị nào?**
- Click "📡 Quét mạng (Demo)" để thêm mẫu
- Hoặc thêm thủ công bằng form

**Quên pass của admin?**
- Xóa file `data/devices.json` để reset

## Tiếp theo

Để upgrade lên production:
- Thêm authentication (JWT)
- Database PostgreSQL
- HTTPS/TLS
- 802.1X + RADIUS
- Quét ARP thực (Python scapy)
- Integration với switch/firewall thật
