using System.Diagnostics;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<DeviceStore>();
builder.Services.AddSingleton<FirewallService>();
var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new
{
    ok = true,
    timestamp = DateTime.UtcNow,
    platform = Environment.OSVersion.Platform,
    machine = Environment.MachineName
}));

app.MapGet("/api/devices", (DeviceStore store) => Results.Ok(store.Devices));

app.MapPost("/api/devices/manual", (ManualDeviceRequest request, DeviceStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Ip) || string.IsNullOrWhiteSpace(request.Mac))
        return Results.BadRequest(new { message = "Ip và MAC bắt buộc" });

    var device = new Device
    {
        Id = Guid.NewGuid().ToString(),
        Name = string.IsNullOrWhiteSpace(request.Name) ? "Manual-Device" : request.Name,
        Ip = request.Ip,
        Mac = request.Mac.ToUpperInvariant(),
        Vendor = request.Vendor ?? "Unknown",
        Status = "pending",
        LastSeen = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    store.Upsert(device);
    return Results.Ok(device);
});

app.MapPost("/api/devices/scan", async (DeviceStore store, FirewallService firewall) =>
{
    var devices = await ArpScannerService.ScanAsync();
    foreach (var d in devices)
    {
        store.Upsert(d);
    }

    return Results.Ok(new
    {
        scanned = devices.Count,
        devices = devices
    });
});

app.MapPatch("/api/devices/{id}/status", (string id, StatusUpdateRequest request, DeviceStore store, FirewallService firewall) =>
{
    var device = store.GetById(id);
    if (device == null) return Results.NotFound(new { message = "Không tìm thấy thiết bị" });

    device.Status = request.Status;
    device.LastSeen = DateTime.UtcNow;

    if (request.Status == "blocked")
    {
        firewall.BlockIp(device.Ip, device.Mac, device.Name);
    }
    else if (request.Status == "approved")
    {
        firewall.UnblockIp(device.Ip, device.Mac);
    }

    return Results.Ok(device);
});

app.MapDelete("/api/devices/{id}", (string id, DeviceStore store, FirewallService firewall) =>
{
    var device = store.GetById(id);
    if (device == null) return Results.NotFound(new { message = "Không tìm thấy thiết bị" });

    firewall.UnblockIp(device.Ip, device.Mac);
    store.Remove(id);
    return Results.NoContent();
});

app.MapGet("/api/firewall/rules", (FirewallService firewall) => Results.Ok(firewall.GetRules()));

app.MapPost("/api/firewall/block", (BlockRuleRequest request, FirewallService firewall) =>
{
    if (string.IsNullOrWhiteSpace(request.Ip) && string.IsNullOrWhiteSpace(request.Mac))
        return Results.BadRequest(new { message = "Cần IP hoặc MAC" });

    var ip = request.Ip ?? "";
    var mac = request.Mac ?? "";
    firewall.BlockIp(ip, mac, request.Name ?? "Manual-Rule");
    return Results.Ok(new { message = "Đã thêm rule chặn" });
});

app.MapPost("/api/firewall/unblock", (BlockRuleRequest request, FirewallService firewall) =>
{
    firewall.UnblockIp(request.Ip ?? "", request.Mac ?? "");
    return Results.Ok(new { message = "Đã gỡ block rule" });
});

app.MapPost("/api/firewall/reset", (FirewallService firewall) =>
{
    firewall.Reset();
    return Results.Ok(new { message = "Đã xóa tất cả firewall rule do NAC tạo" });
});

app.MapFallbackToFile("index.html");

app.Run();

public record ManualDeviceRequest(string? Name, string Ip, string Mac, string? Vendor);
public record StatusUpdateRequest(string Status);
public record BlockRuleRequest(string? Name, string? Ip, string? Mac);

public class DeviceStore
{
    private readonly List<Device> _devices = new();

    public List<Device> Devices => _devices;

    public Device? GetById(string id) => _devices.FirstOrDefault(d => d.Id == id);

    public void Upsert(Device device)
    {
        var existing = _devices.FirstOrDefault(d => d.Mac.Equals(device.Mac, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = device.Name;
            existing.Ip = device.Ip;
            existing.Vendor = device.Vendor;
            existing.LastSeen = device.LastSeen;
            existing.Status = existing.Status == "blocked" ? "blocked" : device.Status;
            return;
        }

        _devices.Insert(0, device);
    }

    public void Remove(string id)
    {
        _devices.RemoveAll(d => d.Id == id);
    }
}

public class Device
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Unknown";
    public string Ip { get; set; } = "";
    public string Mac { get; set; } = "";
    public string Vendor { get; set; } = "Unknown";
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}

public class FirewallRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Ip { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FirewallService
{
    private readonly List<FirewallRule> _rules = new();

    public List<FirewallRule> GetRules() => _rules;

    public void BlockIp(string ip, string mac, string name)
    {
        if (string.IsNullOrWhiteSpace(ip)) return;

        var rule = new FirewallRule
        {
            Ip = ip,
            Mac = mac,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        if (!_rules.Any(r => r.Ip == ip && r.Mac == mac))
        {
            _rules.Add(rule);
        }

        // Windows Firewall cannot reliably block by MAC natively.
        // We block by IP and keep MAC as metadata.
        var escapedIp = ip.Replace("'", "''");
        var ruleName = $"NAC_BLOCK_{ip.Replace('.', '_')}";

        var ps = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"New-NetFirewallRule -DisplayName '{ruleName}' -Direction Inbound -Action Block -RemoteAddress '{escapedIp}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try { Process.Start(ps); }
        catch { /* ignore in development */ }
    }

    public void UnblockIp(string ip, string mac)
    {
        var ruleName = $"NAC_BLOCK_{ip.Replace('.', '_')}";
        var ps = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Remove-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try { Process.Start(ps); }
        catch { /* ignore in development */ }

        _rules.RemoveAll(r => r.Ip == ip || (string.IsNullOrWhiteSpace(ip) && r.Mac == mac));
    }

    public void Reset()
    {
        var ps = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-NetFirewallRule -DisplayName 'NAC_BLOCK_*' | Remove-NetFirewallRule -ErrorAction SilentlyContinue\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try { Process.Start(ps); }
        catch { /* ignore in development */ }

        _rules.Clear();
    }
}

public static class ArpScannerService
{
    public static async Task<List<Device>> ScanAsync()
    {
        var output = await RunArp();
        var devices = ParseArpOutput(output);
        return devices;
    }

    private static async Task<string> RunArp()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "arp",
            Arguments = "-a",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
    }

    private static List<Device> ParseArpOutput(string output)
    {
        var devices = new List<Device>();
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("Interface") && !trimmed.Contains("Internet Address") && !trimmed.Contains("Physical Address"))
            {
                var match = Regex.Match(trimmed, @"^(\d{1,3}(?:\.\d{1,3}){3})\s+\d+\s+\w+\s+([0-9A-Fa-f-]{17}|[0-9A-Fa-f:]{17})");
                if (!match.Success) continue;

                var ip = match.Groups[1].Value;
                var mac = match.Groups[2].Value.Replace('-', ':');
                mac = string.Join(":", mac.Split(':').Select(part => part.Length == 1 ? "0" + part : part).ToArray());

                devices.Add(new Device
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Detected-Device",
                    Ip = ip,
                    Mac = mac.ToUpperInvariant(),
                    Vendor = DetectVendor(mac),
                    Status = "pending",
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return devices
            .GroupBy(d => d.Mac, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private static string DetectVendor(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return "Unknown";
        var prefix = mac.Substring(0, 8).ToUpperInvariant();
        return prefix switch
        {
            "00:1A:2B" => "Hikvision",
            "00:1A:2B" => "Hikvision",
            "00:50:56" => "VMware",
            "00:0C:29" => "VMware",
            "48:8F:5A" => "Intel",
            "A4:B1:C1" => "Intel",
            _ => "Unknown"
        };
    }
}
