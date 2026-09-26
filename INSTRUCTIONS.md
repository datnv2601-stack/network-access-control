# NAC Demo - Hướng dẫn cài đặt và chạy

## 📋 Yêu cầu hệ thống
- Node.js v14+ (tải từ https://nodejs.org/)
- npm (theo kèm với Node.js)
- VS Code (không bắt buộc, có thể dùng terminal bất kỳ)

## 🚀 Cài đặt và chạy

### Bước 1: Tải source code
```bash
# Clone repository
git clone https://github.com/datnv2601-stack/network-access-control.git
cd network-access-control
```

Hoặc download file ZIP từ GitHub và giải nén.

### Bước 2: Cài đặt dependencies
```bash
npm install
```

### Bước 3: Chạy ứng dụng
```bash
npm start
```

Bạn sẽ thấy message:
```
NAC demo is running on http://localhost:3000
```

### Bước 4: Mở trình duyệt
- Truy cập: http://localhost:3000
- Giao diện dashboard sẽ hiện lên

## 📊 Tính năng demo

### 1. Dashboard Summary
- Tổng số thiết bị đang kết nối
- Số thiết bị Approved, Pending, Blocked, Quarantined
- Tự động cập nhật khi thêm/sửa thiết bị

### 2. Quản lý Thiết bị
- **Thêm thiết bị mới**: Nhập tên, IP, MAC, VLAN, vai trò
- **Xem danh sách**: Bảng hiển thị tất cả thiết bị
- **Phê duyệt**: Nút Approve để thay đổi trạng thái
- **Trạng thái**:
  - 🟢 **Approved**: Thiết bị được phép truy cập
  - 🟡 **Pending**: Đang chờ phê duyệt
  - 🔴 **Blocked**: Bị chặn
  - 🟣 **Quarantined**: Cách ly (kiểm tra độ an toàn)

### 3. Chính sách truy cập
- **Thêm policy**: Tên, loại (allow/restrict/block), điều kiện, hành động
- **Danh sách policy**: Hiển thị tất cả quy tắc truy cập
- **Ví dụ policy**:
  - Allow: 192.168.10.0/24 (Staff)
  - Restrict: 192.168.50.0/24 (Guest)
  - Block: 00:1A:2B:* (Thiết bị nghi vấn)

## 🔌 API Endpoints

### GET /api/summary
Lấy thống kê tổng quan
```bash
curl http://localhost:3000/api/summary
```

### GET /api/devices
Lấy danh sách tất cả thiết bị
```bash
curl http://localhost:3000/api/devices
```

### POST /api/devices
Thêm thiết bị mới
```bash
curl -X POST http://localhost:3000/api/devices \
  -H "Content-Type: application/json" \
  -d '{
    "name": "PC-New",
    "ip": "192.168.10.99",
    "mac": "AA:BB:CC:DD:EE:FF",
    "vlan": "VLAN 10",
    "role": "staff",
    "status": "pending",
    "notes": "Máy mới của nhân viên"
  }'
```

### PATCH /api/devices/:id
Cập nhật trạng thái thiết bị
```bash
curl -X PATCH http://localhost:3000/api/devices/1 \
  -H "Content-Type: application/json" \
  -d '{"status": "approved"}'
```

### GET /api/policies
Lấy danh sách chính sách
```bash
curl http://localhost:3000/api/policies
```

### POST /api/policies
Thêm chính sách mới
```bash
curl -X POST http://localhost:3000/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Block-Malware",
    "type": "block",
    "condition": "192.168.99.0/24",
    "action": "block",
    "description": "Chặn subnet nghi vấn"
  }'
```

## 📁 Cấu trúc thư mục

```
network-access-control/
├── server.js              # Backend Express server
├── package.json           # Cấu hình npm
├── public/
│   ├── index.html        # Giao diện chính
│   ├── app.js            # Logic frontend (JavaScript)
│   └── styles.css        # CSS styling
├── README.md             # Tài liệu dự án
└── INSTRUCTIONS.md       # File này
```

## 🛠️ Phát triển

### Dùng Nodemon (tự động reload)
```bash
npm install -g nodemon
npm run dev
```

### Test API với Postman
1. Download Postman từ https://www.postman.com/downloads/
2. Import các endpoint từ mục "API Endpoints" ở trên
3. Test các request

## 🐛 Troubleshooting

### Port 3000 đã được sử dụng
```bash
# Thay đổi port
PORT=3001 npm start
# Hoặc truy cập http://localhost:3001
```

### npm install lỗi
```bash
# Xóa node_modules và package-lock.json
rm -rf node_modules package-lock.json
npm install
```

### Giao diện không tải được
- Kiểm tra console (F12) để xem lỗi
- Đảm bảo server đang chạy (kiểm tra terminal)
- Thử Ctrl+Shift+R để hard refresh trình duyệt

## 📈 Nâng cấp trong tương lai

1. **Database**: Thêm PostgreSQL để lưu trữ persistent
2. **Authentication**: Thêm login, phân quyền user
3. **Real Network Scanning**: Tích hợp ARP scan, DHCP snooping
4. **Firewall Integration**: Kết nối iptables/PF trên Linux/BSD
5. **802.1X Support**: Xác thực port-based
6. **Advanced Policies**: VLAN tagging, QoS, bandwidth limiting
7. **Monitoring**: Realtime traffic analysis, threat detection
8. **Mobile App**: App iOS/Android để quản lý trên điện thoại

## 📝 Ghi chú

- Đây là phiên bản **DEMO** để học tập
- Dữ liệu được lưu **IN-MEMORY** (mất khi restart)
- **KHÔNG dùng cho production** mà không có thêm bảo mật
- Cần thêm authentication, HTTPS, database cho môi trường thực

## 📞 Hỗ trợ

Nếu gặp vấn đề, bạn có thể:
1. Kiểm tra terminal xem lỗi gì
2. Xem browser console (F12 → Console tab)
3. Tham khảo API documentation ở trên

---

**Happy coding! 🚀**
