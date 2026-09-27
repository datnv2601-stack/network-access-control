using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NacPro.Api.Data;
using NacPro.Api.Models;
using NacPro.Api.Services;
using System.Security.Claims;

namespace NacPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly DeviceService _deviceService;
        private readonly AuditService _auditService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(
            ApplicationDbContext context,
            DeviceService deviceService,
            AuditService auditService,
            ILogger<DevicesController> logger)
        {
            _context = context;
            _deviceService = deviceService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Get all devices
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var query = _context.Devices.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d =>
                    d.Name.Contains(search) ||
                    d.IpAddress.Contains(search) ||
                    d.MacAddress.Contains(search));

            var devices = await query
                .OrderByDescending(d => d.LastSeen)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(devices);
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(Guid id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound(new { message = "Thiết bị không tìm thấy" });

            return Ok(device);
        }

        /// <summary>
        /// Create new device
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator,Operator")]
        public async Task<ActionResult<Device>> CreateDevice([FromBody] CreateDeviceRequest request)
        {
            if (await _context.Devices.AnyAsync(d => d.MacAddress == request.MacAddress))
                return BadRequest(new { message = "MAC address đã tồn tại" });

            var device = new Device
            {
                Name = request.Name,
                IpAddress = request.IpAddress,
                MacAddress = request.MacAddress.ToUpper(),
                Vendor = request.Vendor,
                DeviceType = request.DeviceType,
                Status = request.Status ?? "Pending",
                VlanId = request.VlanId,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _auditService.LogAsync(Guid.Parse(userId ?? Guid.Empty.ToString()),
                "Created", "Device", device.Id, null, device.Name);

            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }

        /// <summary>
        /// Update device status
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator,Operator")]
        public async Task<ActionResult<Device>> UpdateDevice(Guid id, [FromBody] UpdateDeviceRequest request)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound(new { message = "Thiết bị không tìm thấy" });

            var oldStatus = device.Status;
            device.Status = request.Status ?? device.Status;
            device.UpdatedAt = DateTime.UtcNow;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _auditService.LogAsync(Guid.Parse(userId ?? Guid.Empty.ToString()),
                "Updated", "Device", device.Id, oldStatus, device.Status);

            return Ok(device);
        }

        /// <summary>
        /// Delete device
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _auditService.LogAsync(Guid.Parse(userId ?? Guid.Empty.ToString()),
                "Deleted", "Device", id, device.Name, null);

            return NoContent();
        }

        /// <summary>
        /// Get device summary
        /// </summary>
        [HttpGet("summary/stats")]
        public async Task<ActionResult<object>> GetDeviceSummary()
        {
            var devices = await _context.Devices.ToListAsync();
            return Ok(new
            {
                total = devices.Count,
                approved = devices.Count(d => d.Status == "Approved"),
                pending = devices.Count(d => d.Status == "Pending"),
                blocked = devices.Count(d => d.Status == "Blocked"),
                quarantined = devices.Count(d => d.Status == "Quarantined"),
                unknown = devices.Count(d => d.Status == "Unknown")
            });
        }
    }

    // Request/Response models
    public class CreateDeviceRequest
    {
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string MacAddress { get; set; }
        public string? Vendor { get; set; }
        public string? DeviceType { get; set; }
        public string? Status { get; set; }
        public int? VlanId { get; set; }
    }

    public class UpdateDeviceRequest
    {
        public string? Status { get; set; }
        public string? Name { get; set; }
        public int? VlanId { get; set; }
    }
}
