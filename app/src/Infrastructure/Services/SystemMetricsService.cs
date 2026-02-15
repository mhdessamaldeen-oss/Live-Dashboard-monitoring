using Application.DTOs.Metrics;
using Application.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

namespace Infrastructure.Services;

/// <summary>
/// System metrics provider using Windows Performance Counters.
/// Should be registered as a Singleton to maintain counter state across collection cycles.
/// </summary>
[SupportedOSPlatform("windows")]
public class SystemMetricsService : ISingletonService
{
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _ramCounter;
    private PerformanceCounter? _diskCounter;
    private bool _initialized = false;
    private readonly object _lock = new();

    public SystemMetricsService()
    {
        if (OperatingSystem.IsWindows())
        {
            InitializeCounters();
        }
    }

    private void InitializeCounters()
    {
        try
        {
            // Note: Accessing Performance Counters might require administrative privileges 
            // or the user to be in the "Performance Monitor Users" group.
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            _diskCounter = new PerformanceCounter("LogicalDisk", "% Free Space", "_Total");
            
            // Warm up: The first call to NextValue always returns 0 for multi-sample counters like CPU.
            // By making this a Singleton, subsequent calls in CollectMetricsAsync will return 
            // the average since the last collection (e.g. 5 seconds ago).
            _cpuCounter.NextValue();
            _ramCounter.NextValue();
            _diskCounter.NextValue();
            
            _initialized = true;
        }
        catch (Exception ex)
        {
            // If initialization fails (e.g. permissions), we'll mark as uninitialized.
            // In a production app, we'd log this specifically.
            _initialized = false;
        }
    }

    public Task<MetricDto> CollectMetricsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        var metric = new MetricDto
        {
            ServerId = serverId,
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 0,
            MemoryUsagePercent = 0, 
            DiskUsagePercent = 0
        };

        if (OperatingSystem.IsWindows())
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    InitializeCounters();
                }

                if (_initialized)
                {
                    try
                    {
                        // NextValue() returns the average since the last time NextValue() was called on THIS instance.
                        var cpu = _cpuCounter?.NextValue() ?? 0;
                        
                        // Occasionally counters might spike or return invalid results on specific hardware.
                        // We'll clamp to 100% just in case of logical anomalies.
                        metric.CpuUsagePercent = Math.Clamp(Math.Round(cpu, 1), 0, 100);
                        
                        metric.MemoryUsagePercent = Math.Round(_ramCounter?.NextValue() ?? 0, 1);
                        
                        // % Free Space -> % Used Space. 
                        // Note: Using LogicalDisk/_Total gives the overall percentage for all drives combined.
                        var freeDisk = _diskCounter?.NextValue() ?? 100;
                        metric.DiskUsagePercent = Math.Clamp(Math.Round(100 - freeDisk, 1), 0, 100);
                    }
                    catch
                    {
                        // In case of transient permission or access errors, reset init to try again next time
                        _initialized = false;
                    }
                }
            }
        }

        // Always attempt to get detailed drive info using DriveInfo (regardless of counter initialization)
        CollectDriveInfo(metric);

        // Core fields mapping
        metric.CpuUsage = metric.CpuUsagePercent;
        metric.MemoryUsage = metric.MemoryUsagePercent;
        metric.DiskUsage = metric.DiskUsagePercent;

        // System details
        metric.ActiveProcesses = Process.GetProcesses().Length;
        metric.SystemUptime = Environment.TickCount64 / 1000.0;

        return Task.FromResult(metric);
    }

    private void CollectDriveInfo(MetricDto metric)
    {
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).ToList();
            foreach (var drive in drives)
            {
                var total = drive.TotalSize / 1024 / 1024; // MB
                var free = drive.TotalFreeSpace / 1024 / 1024; // MB
                var used = total - free;
                var percent = total > 0 ? (double)used / total * 100 : 0;

                metric.Disks.Add(new DiskDto
                {
                    DriveLetter = drive.Name,
                    TotalSpaceMB = total,
                    FreeSpaceMB = free,
                    UsedPercentage = Math.Round(percent, 1)
                });
            }
        }
        catch { /* Suppress IO errors for specific drives */ }
    }
}
