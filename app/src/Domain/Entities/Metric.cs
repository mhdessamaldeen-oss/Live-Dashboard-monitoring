using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a point-in-time metric snapshot for a server
/// </summary>
public class Metric : BaseEntity
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long MemoryTotalBytes { get; set; }
    public double NetworkInBytesPerSec { get; set; }
    public double NetworkOutBytesPerSec { get; set; }
    public int ActiveProcesses { get; set; }
    public double SystemUptime { get; set; } // in seconds
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int ServerId { get; set; }
}
