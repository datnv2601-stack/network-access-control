using Microsoft.EntityFrameworkCore;
using NacPro.Api.Models;

namespace NacPro.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<Policy> Policies { get; set; } = null!;
        public DbSet<BlockRule> BlockRules { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<NetworkIntegration> NetworkIntegrations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default users
            var adminId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();

            var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            var operatorPassword = BCrypt.Net.BCrypt.HashPassword("Operator@123");
            var viewerPassword = BCrypt.Net.BCrypt.HashPassword("Viewer@123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminId,
                    Username = "admin",
                    Email = "admin@nac.local",
                    PasswordHash = adminPassword,
                    Role = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = operatorId,
                    Username = "operator",
                    Email = "operator@nac.local",
                    PasswordHash = operatorPassword,
                    Role = "Operator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = viewerId,
                    Username = "viewer",
                    Email = "viewer@nac.local",
                    PasswordHash = viewerPassword,
                    Role = "Viewer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed default policies
            modelBuilder.Entity<Policy>().HasData(
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "Office-Staff",
                    Type = "Allow",
                    Condition = "192.168.1.0/24",
                    Action = "allow",
                    Description = "Cho phép nhân viên văn phòng",
                    IsActive = true,
                    Priority = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "Guest-WiFi",
                    Type = "Restrict",
                    Condition = "192.168.50.0/24",
                    Action = "restricted",
                    Description = "Chỉ cho phép Internet, cách ly nội bộ",
                    IsActive = true,
                    Priority = 90,
                    CreatedAt = DateTime.UtcNow
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "IoT-Quarantine",
                    Type = "Block",
                    Condition = "00:1A:2B:*",
                    Action = "quarantine",
                    Description = "Cách ly thiết bị IoT đã cấu hình sai",
                    IsActive = true,
                    Priority = 80,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
