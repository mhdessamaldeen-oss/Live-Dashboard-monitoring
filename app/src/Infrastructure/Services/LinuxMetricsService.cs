using Application.DTOs.Metrics;
using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Collects high-precision system metrics on Linux environments by reading and parsing the kernel's /proc filesystem.
/// This service implements a sampling pattern (singleton) to calculate usage deltas over time (CPU/Network).
/// </summary>
public class LinuxMetricsService : ISingletonService
{
    // Sampling state for delta-based calculations
    private (long Total, long Idle)? _lastCpuStat;
    private (long Rx, long Tx)? _lastNetStat;
    private DateTime? _lastCpuTimestamp;
    private DateTime? _lastNetTimestamp;
    private readonly object _lock = new();

    /// <summary>
    /// Aggregates CPU, Memory, Disk, and Network metrics into a single snapshot.
    /// Uses thread-safe sampling to ensure data accuracy across concurrent requests.
    /// </summary>
    public Task<MetricDto> CollectMetricsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        var metric = new MetricDto
        {
            ServerId = serverId,
            Timestamp = DateTime.UtcNow,
            Disks = new List<DiskDto>()
        };

        lock (_lock)
        {
            // 1. CPU Usage: Calculated via deltas from /proc/stat
            UpdateCpuUsage(metric);

            // 2. Memory Info: Parsed from /proc/meminfo
            ReadMemoryInfo(metric);

            // 3. Network Throughput: Calculated via byte deltas from /proc/net/dev
            UpdateNetworkInfo(metric);
        }

        // 4. Disk Utilization: Driven by .NET DriveInfo abstractions
        ReadDiskInfo(metric);

        // 5. Process Count: Count of numeric directories in /proc represents active PIDs
        try
        {
            var procDirs = Directory.GetDirectories("/proc")
                .Count(d => int.TryParse(Path.GetFileName(d), out _));
            metric.ActiveProcesses = procDirs;
        }
        catch { metric.ActiveProcesses = 0; }

        // 6. System Uptime: Parsed from /proc/uptime (first parameter is seconds since boot)
        try
        {
            var uptimeText = File.ReadAllText("/proc/uptime").Trim();
            var parts = uptimeText.Split(' ');
            if (double.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var uptime))
            {
                metric.SystemUptime = uptime;
            }
        }
        catch { metric.SystemUptime = 0; }

        // Core DTO Field Synchronization
        metric.CpuUsage = metric.CpuUsagePercent;
        metric.MemoryUsage = metric.MemoryUsagePercent;
        metric.DiskUsage = metric.DiskUsagePercent;
        metric.NetworkIn = metric.NetworkInBytesPerSec;
        metric.NetworkOut = metric.NetworkOutBytesPerSec;

        return Task.FromResult(metric);
    }

    /// <summary>
    /// Calculates average CPU utilization by comparing total and idle ticks since the last collection cycle.
    /// Formula: 1 - (delta_idle / delta_total)
    /// </summary>
    private void UpdateCpuUsage(MetricDto metric)
    {
        try
        {
            var now = DateTime.UtcNow;
            var current = ReadCpuStat();

            if (!_lastCpuStat.HasValue || !_lastCpuTimestamp.HasValue)
            {
                // Edge Case: First run has no baseline. We force a small wait to provide immediate data 
                // rather than returning 0% for the first collection cycle.
                Thread.Sleep(100);
                var second = ReadCpuStat();
                var totalDelta = second.Total - current.Total;
                var idleDelta = second.Idle - current.Idle;

                if (totalDelta > 0)
                {
                    var usage = (1.0 - (double)idleDelta / totalDelta) * 100;
                    metric.CpuUsagePercent = Math.Clamp(Math.Round(usage, 1), 0, 100);
                }
                
                _lastCpuStat = second;
            }
            else
            {
                var totalDelta = current.Total - _lastCpuStat.Value.Total;
                var idleDelta = current.Idle - _lastCpuStat.Value.Idle;

                if (totalDelta > 0)
                {
                    var usage = (1.0 - (double)idleDelta / totalDelta) * 100;
                    metric.CpuUsagePercent = Math.Clamp(Math.Round(usage, 1), 0, 100);
                }
                
                _lastCpuStat = current;
            }

            _lastCpuTimestamp = now;
        }
        catch { metric.CpuUsagePercent = 0; }
    }

    /// <summary>
    /// Reads the first line of /proc/stat to extract cumulative CPU ticks.
    /// </summary>
    private static (long Total, long Idle) ReadCpuStat()
    {
        var line = File.ReadLines("/proc/stat").First();
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var values = parts.Skip(1).Select(long.Parse).ToArray();
        // Index 3 is 'idle', Index 4 is 'iowait' (both considered non-busy time in this context)
        var idle = values[3] + values[4];
        var total = values.Sum();
        return (total, idle);
    }

    /// <summary>
    /// Calculates network throughput (Bytes/Sec) by measuring the rate of change in interface byte counters.
    /// </summary>
    private void UpdateNetworkInfo(MetricDto metric)
    {
        try
        {
            var current = ReadNetBytes();
            var now = DateTime.UtcNow;

            if (_lastNetStat.HasValue && _lastNetTimestamp.HasValue)
            {
                var seconds = (now - _lastNetTimestamp.Value).TotalSeconds;
                if (seconds > 0)
                {
                    metric.NetworkInBytesPerSec = Math.Round((current.Rx - _lastNetStat.Value.Rx) / seconds, 0);
                    metric.NetworkOutBytesPerSec = Math.Round((current.Tx - _lastNetStat.Value.Tx) / seconds, 0);
                }
            }

            _lastNetStat = current;
            _lastNetTimestamp = now;
        }
        catch
        {
            metric.NetworkInBytesPerSec = 0;
            metric.NetworkOutBytesPerSec = 0;
        }
    }

    /// <summary>
    /// Sums Received and Transmitted bytes across all active network interfaces (excluding loopback).
    /// </summary>
    private static (long Rx, long Tx) ReadNetBytes()
    {
        long rxTotal = 0, txTotal = 0;
        var lines = File.ReadAllLines("/proc/net/dev").Skip(2);
        foreach (var line in lines)
        {
            var parts = line.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length < 2) continue;
            var iface = parts[0].Trim();
            if (iface == "lo") continue;
            var values = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length >= 10)
            {
                rxTotal += long.Parse(values[0]); // Index 0: Receive Bytes
                txTotal += long.Parse(values[8]); // Index 8: Transmit Bytes
            }
        }
        return (rxTotal, txTotal);
    }

    /// <summary>
    /// Parses /proc/meminfo to calculate memory utilization.
    /// Uses 'MemAvailable' for high accuracy as it accounts for cache/buffer recovery potential.
    /// </summary>
    private void ReadMemoryInfo(MetricDto metric)
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            long memTotal = 0, memAvailable = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:")) memTotal = ParseMemInfoValue(line);
                else if (line.StartsWith("MemAvailable:")) memAvailable = ParseMemInfoValue(line);
            }

            if (memTotal > 0)
            {
                metric.MemoryTotalBytes = memTotal * 1024;
                var used = memTotal - memAvailable;
                metric.MemoryUsedBytes = used * 1024;
                metric.MemoryUsagePercent = Math.Clamp(Math.Round((double)used / memTotal * 100, 1), 0, 100);
            }
        }
        catch { }
    }

    private static long ParseMemInfoValue(string line)
    {
        var valuePart = line.Split(':', StringSplitOptions.TrimEntries)[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return long.Parse(valuePart[0]);
    }

    /// <summary>
    /// Iterates through fixed drives on the system and aggregates space utilization data.
    /// </summary>
    private void ReadDiskInfo(MetricDto metric)
    {
        try
        {
            // Filter to real fixed drives with substantial size (ignore small virtual mounts like /etc/hosts in Docker)
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed && d.TotalSize > 100 * 1024 * 1024)
                .ToList();
                
            double totalSpaceMB = 0, usedSpaceMB = 0;
            foreach (var drive in drives)
            {
                var totalMB = drive.TotalSize / (1024 * 1024);
                var freeMB = drive.AvailableFreeSpace / (1024 * 1024);
                var usedMB = totalMB - freeMB;
                
                totalSpaceMB += totalMB;
                usedSpaceMB += usedMB;
                
                metric.Disks.Add(new DiskDto 
                { 
                    DriveLetter = drive.Name, 
                    TotalSpaceMB = totalMB, 
                    FreeSpaceMB = freeMB, 
                    UsedPercentage = totalMB > 0 ? (double)usedMB / totalMB * 100 : 0 
                });
            }
            metric.DiskUsagePercent = totalSpaceMB > 0 ? Math.Clamp(Math.Round(usedSpaceMB / totalSpaceMB * 100, 1), 0, 100) : 0;
        }
        catch { }
    }
}
