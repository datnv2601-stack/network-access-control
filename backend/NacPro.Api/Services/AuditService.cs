using NacPro.Api.Data;
using NacPro.Api.Models;

namespace NacPro.Api.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(Guid userId, string action, string entity, Guid entityId, string? oldValue, string? newValue)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }

    public class DeviceService
    {
        private readonly ApplicationDbContext _context;

        public DeviceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Device?> FindByMacAsync(string macAddress)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.MacAddress == macAddress.ToUpper());
        }

        public async Task<Device?> FindByIpAsync(string ipAddress)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.IpAddress == ipAddress);
        }

        public async Task AddOrUpdateAsync(Device device)
        {
            var existing = await FindByMacAsync(device.MacAddress);
            if (existing != null)
            {
                existing.IpAddress = device.IpAddress;
                existing.LastSeen = DateTime.UtcNow;
                _context.Devices.Update(existing);
            }
            else
            {
                _context.Devices.Add(device);
            }
            await _context.SaveChangesAsync();
        }
    }
}
