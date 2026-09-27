using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NacPro.Api.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Viewer"; // Admin, Operator, Viewer

        public bool IsActive { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    [Table("Devices")]
    public class Device
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(17)]
        [Index(IsUnique = true)]
        public string MacAddress { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Vendor { get; set; }

        [StringLength(50)]
        public string? DeviceType { get; set; } // PC, Camera, NVR, Printer, Switch

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Unknown"; // Approved, Pending, Blocked, Quarantined, Unknown

        public int? VlanId { get; set; }

        public int? Port { get; set; }

        public Guid? SwitchId { get; set; }

        public bool IsBlocked { get; set; } = false;

        public string? BlockReason { get; set; }

        [Required]
        public DateTime FirstSeen { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<BlockRule> BlockRules { get; set; } = new List<BlockRule>();
    }

    [Table("Policies")]
    public class Policy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        [Index(IsUnique = true)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Allow, Restrict, Block, Quarantine

        [Required]
        [StringLength(500)]
        public string Condition { get; set; } = string.Empty; // IP range or MAC pattern

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    [Table("BlockRules")]
    public class BlockRule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? DeviceId { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(17)]
        public string? MacAddress { get; set; }

        public string? Reason { get; set; }

        [Required]
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UnblockedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Blocked, Unblocked

        [StringLength(100)]
        public string? Entity { get; set; } // Device, Policy, BlockRule

        public Guid? EntityId { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("NetworkIntegrations")]
    public class NetworkIntegration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Switch, Firewall, Kaspersky, Camera, NVR

        [StringLength(100)]
        public string? Vendor { get; set; } // Cisco, HPE, Palo Alto, Hikvision, etc

        [StringLength(100)]
        public string? Model { get; set; }

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        public int? Port { get; set; }

        [StringLength(255)]
        public string? Username { get; set; }

        public string? PasswordEncrypted { get; set; }

        public string? ApiKey { get; set; }

        [StringLength(50)]
        public string? Protocol { get; set; } // SNMP, SSH, REST, HTTPS, ONVIF

        [StringLength(50)]
        public string ConnectionStatus { get; set; } = "Unknown"; // Connected, Failed, Timeout

        public DateTime? LastChecked { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
