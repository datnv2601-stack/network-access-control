# NAC Controller Demo

## Chạy bằng VS Code

Yêu cầu Node.js 18 trở lên.

```bash
npm install
npm start
```

Mở http://localhost:3000.

Chế độ phát triển tự reload:

```bash
npm run dev
```

## Có gì trong bản này

- Dashboard thống kê thiết bị.
- Thêm, tìm kiếm, approve, block, xóa thiết bị.
- Kiểm tra định dạng IPv4 và MAC.
- Quản lý policy.
- Audit events.
- Dữ liệu được lưu tại `data/store.json`, không mất khi restart.
- API health: `GET /api/health`.

## Lưu ý an toàn

Đây là bản lab chạy local. Nó **chưa tự động cấu hình switch/firewall, chưa phải NAC production và không có 802.1X/RADIUS**. Không đưa lên mạng thật hoặc dùng để chặn thiết bị thật nếu chưa bổ sung xác thực, HTTPS, phân quyền, database, backup và connector đúng model thiết bị.
