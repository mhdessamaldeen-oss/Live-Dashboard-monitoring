using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Server : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? OperatingSystem { get; set; }
    public ServerStatus Status { get; set; } = ServerStatus.Unknown;
    public bool IsActive { get; set; } = true;
    public bool IsHost { get; set; } = false;
    
    // Threshold settings
    public double CpuWarningThreshold { get; set; } = 70.0;
    public double CpuCriticalThreshold { get; set; } = 90.0;
    public double MemoryWarningThreshold { get; set; } = 70.0;
    public double MemoryCriticalThreshold { get; set; } = 90.0;
    public double DiskWarningThreshold { get; set; } = 80.0;
    public double DiskCriticalThreshold { get; set; } = 95.0;
}
