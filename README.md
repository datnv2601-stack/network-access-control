# NAC Pro - C# ASP.NET Core + React Dashboard

**Production-Ready Network Access Control System for Windows**

## 🎯 Tính năng

✅ **ARP Scanner** - Tự động quét IP/MAC thực tế từ LAN  
✅ **Multi-Tab Dashboard** - Dashboard, Devices, Policies, Events, Logs, Integrations, Settings  
✅ **SQL Server Database** - LocalDB hoặc Express  
✅ **JWT Authentication** - Login with roles (Admin, Operator, Viewer)  
✅ **HTTPS/TLS** - Self-signed certificate  
✅ **Windows Firewall Integration** - Block/Unblock IP/MAC  
✅ **Real-time Updates** - WebSocket/SignalR  
✅ **Audit Logging** - Tất cả thao tác được ghi  
✅ **Backup/Restore** - Database backup tự động  
✅ **REST API** - Fully documented  
✅ **Role-Based Access Control** - Admin, Operator, Viewer  
✅ **Policy Engine** - Rule-based device management  

## 📋 Yêu cầu

- **OS**: Windows 10/11 hoặc Windows Server 2019+
- **IDE**: Visual Studio 2022 Community (free) hoặc VS Code
- **.NET SDK**: .NET 7.0 LTS ho���c .NET 8.0
- **Database**: SQL Server Express 2019+ (free) hoặc LocalDB
- **Node.js**: 18+ (Frontend)
- **Git**: Để clone repo

## 🚀 Cài đặt nhanh

### 1. Clone repository

```bash
git clone https://github.com/datnv2601-stack/network-access-control.git
cd network-access-control
git checkout nac-pro-dotnet
```

### 2. Cài .NET SDK

```bash
# Kiểm tra
dotnet --version

# Nếu chưa có: https://dotnet.microsoft.com/download
```

### 3. Cài SQL Server LocalDB

```bash
# LocalDB đi kèm Visual Studio, hoặc download:
# https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb

# Hoặc dùng Docker:
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Nac@Prod123" -p 1433:1433 mcr.microsoft.com/mssql/server:latest
```

### 4. Setup Backend

```bash
cd backend/NacPro.Api

# Restore packages
dotnet restore

# Update database
dotnet ef database update

# Run backend
dotnet run --launch-profile https
```

Backend chạy tại: `https://localhost:5001`

### 5. Setup Frontend (Terminal khác)

```bash
cd frontend
npm install
npm start
```

Frontend chạy tại: `http://localhost:3000`

### 6. Đăng nhập

```
Username: admin
Password: Admin@123
Role: Administrator
```

## 📁 Cấu trúc Project

```
nac-pro-dotnet/
├── backend/
│   ├── NacPro.Api/
│   │   ├── Program.cs                 # Startup configuration
│   │   ├── appsettings.json          # Configuration
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── DevicesController.cs
│   │   │   ├── PoliciesController.cs
│   │   │   ├── FirewallController.cs
│   │   │   ├── ScannerController.cs
│   │   │   ├── AuditController.cs
│   │   │   └── IntegrationsController.cs
│   │   ├── Models/
│   │   │   ├── User.cs
│   │   │   ├── Device.cs
│   │   │   ├── Policy.cs
│   │   │   ├── BlockRule.cs
│   │   │   ├── AuditLog.cs
│   │   │   └── NetworkIntegration.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── DeviceService.cs
│   │   │   ├── ArpScannerService.cs   # ARP scanner for Windows
│   │   │   ├── FirewallService.cs     # Windows Firewall integration
│   │   │   ├── PolicyService.cs
│   │   │   ├── AuditService.cs
│   │   │   ├��─ BackupService.cs
│   │   │   └── IntegrationService.cs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Migrations/
│   │   │   └── SeedData.cs
│   │   ├── Middleware/
│   │   │   ├── JwtMiddleware.cs
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── AuditMiddleware.cs
│   │   │   └── ExceptionMiddleware.cs
│   │   ├── Connectors/
│   │   │   ├── IDeviceConnector.cs
│   │   │   ├── GenericHttpConnector.cs
│   │   │   ├── Cisco/CiscoConnector.cs
│   │   │   ├── PaloAlto/PaloAltoConnector.cs
│   │   │   ├── Kaspersky/KasperskyConnector.cs
│   │   │   └── Hikvision/HikvisionConnector.cs
│   │   ├── Certificates/
│   │   │   ├── nac-dev.pfx
│   │   │   └── GenerateCertificate.ps1
│   │   ├── NacPro.Api.csproj
│   │   └── appsettings.Production.json
│   ├── NacPro.Tests/
│   │   ├── AuthServiceTests.cs
│   │   ├── DeviceServiceTests.cs
│   │   ├── FirewallServiceTests.cs
│   │   └── NacPro.Tests.csproj
│   └── NacPro.sln
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   ├── Layout/
│   │   │   │   ├── Header.tsx
│   │   │   │   ├── Sidebar.tsx
│   │   │   │   └── TabNavigation.tsx
│   │   │   ├── Dashboard/
│   │   │   │   └── DashboardTab.tsx
│   │   │   ├── Devices/
│   │   │   │   ├── DevicesTab.tsx
│   │   │   │   ├── DeviceList.tsx
│   │   │   │   ├── AddDevice.tsx
│   │   │   │   └── DeviceDetails.tsx
│   │   │   ├── Policies/
│   │   │   │   ├── PoliciesTab.tsx
│   │   │   │   ├── PolicyList.tsx
│   │   │   │   └── AddPolicy.tsx
│   │   │   ├── Events/
│   │   │   │   └── EventsTab.tsx
│   │   │   ├── Logs/
│   │   │   │   └── LogsTab.tsx
│   │   │   ├── Integrations/
│   │   │   │   └── IntegrationsTab.tsx
│   │   │   ├── Settings/
│   │   │   │   ├── SettingsTab.tsx
│   │   │   │   ├── UserManagement.tsx
│   │   │   │   ├── Backup.tsx
│   │   │   │   └── SystemConfig.tsx
│   │   │   ├── Common/
│   │   │   │   ├── LoadingSpinner.tsx
│   │   │   │   ├── ErrorBoundary.tsx
│   │   │   │   ├── ConfirmDialog.tsx
│   │   │   │   └── Toast.tsx
│   │   │   └── Auth/
│   │   │       └── LoginPage.tsx
│   │   ├── services/
│   │   │   ├── api.ts
│   │   │   ├── authService.ts
│   │   │   ├── deviceService.ts
│   │   │   ├── policyService.ts
│   │   │   ├── auditService.ts
│   │   │   └── integrationService.ts
│   │   ├── store/
│   │   │   ├── authSlice.ts
│   │   │   ├── deviceSlice.ts
│   │   │   ├── policySlice.ts
│   │   │   └── store.ts
│   │   ├── types/
│   │   │   ├── index.ts
│   │   │   ├── device.ts
│   │   │   ├── policy.ts
│   │   │   ├── user.ts
│   │   │   └── audit.ts
│   │   ├── hooks/
│   │   │   ├── useAuth.ts
│   │   │   ├── useDevice.ts
│   │   │   └── usePolicy.ts
│   │   ├── styles/
│   │   │   ├── globals.css
│   │   │   ├── variables.css
│   │   │   └── responsive.css
│   │   ├── App.tsx
│   │   ├── index.tsx
│   │   └── config.ts
│   ├── public/
│   ├── package.json
│   └── tsconfig.json
├── docs/
│   ├── API.md
│   ├── SETUP.md
│   ├── DATABASE.md
│   ├── SECURITY.md
│   ├── CONNECTOR_GUIDE.md
│   └── DEPLOYMENT.md
├── .gitignore
└── README.md
```

## 🔑 Tính năng chính

### 1. **ARP Scanner**
- Tự động quét LAN m 30 giây
- Phát hiện IP, MAC, Vendor
- Lưu vào database SQL Server
- Cập nhật trạng thái thiết bị

### 2. **Multi-Tab Dashboard**

**Tab 1: Dashboard**
- Thống kê tổng thiết bị
- Biểu đồ trạng thái
- Device activity realtime
- Quick actions

**Tab 2: Devices**
- Danh sách thiết bị (phát hiện được từ ARP)
- Filter, search, sort
- Approve/Block/Quarantine
- Device details panel
- Add manual device

**Tab 3: Policies**
- Danh sách policy
- Create/Edit/Delete policy
- Priority management
- Policy preview

**Tab 4: Events**
- Realtime device detection
- Device connection/disconnection
- Status changes
- Policy violations

**Tab 5: Logs (Audit)**
- Audit trail
- User actions
- Firewall changes
- Filter by user/action/date
- Export logs

**Tab 6: Integrations**
- Add Switch (Cisco, HPE, Juniper)
- Add Firewall (Palo Alto, FortiGate)
- Add Kaspersky
- Add Camera/NVR
- Test connection
- Sync data

**Tab 7: Settings**
- User Management
- System Configuration
- Backup/Restore
- Scanner Settings
- Firewall Rules
- Certificates

### 3. **Authentication & Authorization**
- Login with username/password
- JWT tokens (15 min expiry, 7 day refresh)
- Roles: Admin, Operator, Viewer
- Permission-based access

### 4. **Windows Firewall Integration**

```csharp
// When blocking a device
New-NetFirewallRule -DisplayName "NAC-Block-192.168.1.10" `
  -RemoteAddress 192.168.1.10 `
  -Direction Inbound `
  -Action Block
```

### 5. **Database Schema**

```sql
-- Users
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(255) UNIQUE,
    PasswordHash NVARCHAR(MAX),
    Role NVARCHAR(50), -- Admin, Operator, Viewer
    IsActive BIT,
    LastLogin DATETIME2,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

-- Devices (from ARP scan)
CREATE TABLE Devices (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255),
    IpAddress NVARCHAR(50) UNIQUE,
    MacAddress NVARCHAR(17) UNIQUE,
    Vendor NVARCHAR(255),
    DeviceType NVARCHAR(50),
    Status NVARCHAR(50), -- Approved, Pending, Blocked, Quarantined
    VlanId INT,
    SwitchId UNIQUEIDENTIFIER,
    IsBlocked BIT,
    BlockReason NVARCHAR(MAX),
    FirstSeen DATETIME2,
    LastSeen DATETIME2,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

-- Policies
CREATE TABLE Policies (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) UNIQUE NOT NULL,
    Type NVARCHAR(50), -- Allow, Restrict, Block, Quarantine
    Condition NVARCHAR(500), -- IP range or MAC pattern
    Action NVARCHAR(50),
    Description NVARCHAR(MAX),
    IsActive BIT,
    Priority INT,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

-- Block Rules
CREATE TABLE BlockRules (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DeviceId UNIQUEIDENTIFIER,
    IpAddress NVARCHAR(50),
    MacAddress NVARCHAR(17),
    Reason NVARCHAR(MAX),
    BlockedAt DATETIME2,
    UnblockedAt DATETIME2,
    IsActive BIT,
    CreatedAt DATETIME2
);

-- Audit Logs
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER,
    Action NVARCHAR(255), -- Created, Updated, Blocked, Unblocked
    Entity NVARCHAR(100), -- Device, Policy, BlockRule
    EntityId UNIQUEIDENTIFIER,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    CreatedAt DATETIME2
);
```

## 📡 API Endpoints

### Auth
```bash
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh
GET    /api/auth/me
```

### Devices
```bash
GET    /api/devices
GET    /api/devices/{id}
POST   /api/devices
PATCH  /api/devices/{id}
DELETE /api/devices/{id}
GET    /api/devices/scan/status
POST   /api/devices/scan/start
POST   /api/devices/scan/stop
```

### Policies
```bash
GET    /api/policies
POST   /api/policies
PATCH  /api/policies/{id}
DELETE /api/policies/{id}
```

### Firewall
```bash
GET    /api/firewall/rules
POST   /api/firewall/rules
DELETE /api/firewall/rules/{id}
POST   /api/firewall/apply
POST   /api/firewall/reset
```

### Audit
```bash
GET    /api/audit/logs
GET    /api/audit/logs/export
DELETE /api/audit/logs/{id}
```

## 🔐 Bảo mật

✅ JWT Authentication  
✅ HTTPS/TLS 1.2+  
✅ Password Hashing (bcrypt, 12 rounds)  
✅ CORS Configuration  
✅ SQL Injection Prevention (Entity Framework)  
✅ XSS Protection (Content Security Policy)  
✅ Rate Limiting (100 req/min per IP)  
✅ Audit Logging  
✅ Role-Based Access Control  
✅ Secrets Management (Environment variables)  

## 🚀 Deployment

### Windows IIS
```bash
cd backend/NacPro.Api
dotnet publish -c Release -o ./publish
# Copy 'publish' folder to IIS
```

### Docker
```bash
docker build -t nac-pro .
docker run -p 5001:5001 nac-pro
```

### Azure App Service
```bash
az login
az webapp up --name nac-pro --resource-group nac-rg --runtime "dotnet:7.0"
```

## 📚 Documentation

- [API Documentation](./docs/API.md)
- [Setup Guide](./docs/SETUP.md)
- [Database Schema](./docs/DATABASE.md)
- [Security Guide](./docs/SECURITY.md)
- [Connector Development](./docs/CONNECTOR_GUIDE.md)
- [Deployment Guide](./docs/DEPLOYMENT.md)

## 🧪 Testing

```bash
cd backend/NacPro.Tests
dotnet test
```

## 📝 Default Credentials

```
Username: admin
Password: Admin@123
Role: Administrator

Username: operator
Password: Operator@123
Role: Operator

Username: viewer
Password: Viewer@123
Role: Viewer
```

⚠️ **Thay đổi password ngay sau lần đăng nhập đầu tiên!**

---

**Ready to deploy enterprise NAC on Windows! 🚀**
