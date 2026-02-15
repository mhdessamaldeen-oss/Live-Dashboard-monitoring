namespace Application.DTOs.Metrics;

public class MetricDto
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long MemoryTotalBytes { get; set; }
    public double NetworkInBytesPerSec { get; set; }
    public double NetworkOutBytesPerSec { get; set; }
    public int ActiveProcesses { get; set; }
    public double SystemUptime { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Status { get; set; }
    
    // Calculated for frontend compatibility
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkIn { get; set; }
    public double NetworkOut { get; set; }
    
    public List<DiskDto> Disks { get; set; } = new();
}

public record MetricsQuery(
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 100);

public record LatestMetricsDto(
    MetricDto? Metric);
