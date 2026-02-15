using Application.DTOs.Metrics;
using Application.Interfaces;

namespace Infrastructure.Services;

public class MockMetricsProvider : ISingletonService
{
    private readonly Random _random = new();

    public Task<MetricDto> CollectMetricsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        // Simulate realistic-looking metrics
        var metric = new MetricDto
        {
            ServerId = serverId,
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = Math.Round(_random.NextDouble() * 100, 1),
            MemoryUsagePercent = Math.Round(_random.NextDouble() * 100, 1),
            DiskUsagePercent = Math.Round(_random.NextDouble() * 100, 1),
            MemoryUsedBytes = (long)(_random.NextDouble() * 16 * 1024 * 1024 * 1024),
            MemoryTotalBytes = 16L * 1024 * 1024 * 1024,
            NetworkInBytesPerSec = Math.Round(_random.NextDouble() * 1000, 0),
            NetworkOutBytesPerSec = Math.Round(_random.NextDouble() * 1000, 0),
            ActiveProcesses = _random.Next(50, 200),
            SystemUptime = Environment.TickCount64 / 1000.0,
            Disks = new List<DiskDto>()
        };

        // Consistency with compatibility fields
        metric.CpuUsage = metric.CpuUsagePercent;
        metric.MemoryUsage = metric.MemoryUsagePercent;
        metric.DiskUsage = metric.DiskUsagePercent;
        metric.NetworkIn = metric.NetworkInBytesPerSec;
        metric.NetworkOut = metric.NetworkOutBytesPerSec;

        // Simulate disks
        metric.Disks.Add(new DiskDto 
        { 
            DriveLetter = "C:", 
            TotalSpaceMB = 500000, 
            FreeSpaceMB = (long)(500000 * (1 - metric.DiskUsagePercent / 100)), 
            UsedPercentage = metric.DiskUsagePercent 
        });
        
        metric.Disks.Add(new DiskDto 
        { 
            DriveLetter = "D:", 
            TotalSpaceMB = 1000000, 
            FreeSpaceMB = (long)(1000000 * 0.8), 
            UsedPercentage = 20.0 
        });

        return Task.FromResult(metric);
    }
}
