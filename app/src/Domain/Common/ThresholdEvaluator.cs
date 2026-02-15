using Domain.Entities;
using Domain.Enums;

namespace Domain.Common;

/// <summary>
/// Domain rules for threshold evaluation and alert generation
/// </summary>
public static class ThresholdEvaluator
{
    /// <summary>
    /// Evaluates CPU metric against server thresholds
    /// </summary>
    public static (bool ShouldAlert, AlertSeverity Severity, string Message) EvaluateCpu(
        Server server, 
        double cpuUsagePercent)
    {
        if (cpuUsagePercent >= server.CpuCriticalThreshold)
        {
            return (true, AlertSeverity.Critical, 
                $"CPU usage critical: {cpuUsagePercent:F1}% (threshold: {server.CpuCriticalThreshold}%)");
        }
        
        if (cpuUsagePercent >= server.CpuWarningThreshold)
        {
            return (true, AlertSeverity.Warning, 
                $"CPU usage warning: {cpuUsagePercent:F1}% (threshold: {server.CpuWarningThreshold}%)");
        }
        
        return (false, AlertSeverity.Info, string.Empty);
    }

    /// <summary>
    /// Evaluates Memory metric against server thresholds
    /// </summary>
    public static (bool ShouldAlert, AlertSeverity Severity, string Message) EvaluateMemory(
        Server server, 
        double memoryUsagePercent)
    {
        if (memoryUsagePercent >= server.MemoryCriticalThreshold)
        {
            return (true, AlertSeverity.Critical, 
                $"Memory usage critical: {memoryUsagePercent:F1}% (threshold: {server.MemoryCriticalThreshold}%)");
        }
        
        if (memoryUsagePercent >= server.MemoryWarningThreshold)
        {
            return (true, AlertSeverity.Warning, 
                $"Memory usage warning: {memoryUsagePercent:F1}% (threshold: {server.MemoryWarningThreshold}%)");
        }
        
        return (false, AlertSeverity.Info, string.Empty);
    }

    /// <summary>
    /// Evaluates Disk usage against server thresholds
    /// </summary>
    public static (bool ShouldAlert, AlertSeverity Severity, string Message) EvaluateDisk(
        Server server, 
        double diskUsagePercent)
    {
        if (diskUsagePercent >= server.DiskCriticalThreshold)
        {
            return (true, AlertSeverity.Critical, 
                $"Disk usage critical: {diskUsagePercent:F1}% (threshold: {server.DiskCriticalThreshold}%)");
        }
        
        if (diskUsagePercent >= server.DiskWarningThreshold)
        {
            return (true, AlertSeverity.Warning, 
                $"Disk usage warning: {diskUsagePercent:F1}% (threshold: {server.DiskWarningThreshold}%)");
        }
        
        return (false, AlertSeverity.Info, string.Empty);
    }

    /// <summary>
    /// Determines the overall server status based on current metrics
    /// </summary>
    public static ServerStatus DetermineServerStatus(
        double cpuUsagePercent,
        double memoryUsagePercent,
        double? diskUsagePercent,
        Server server)
    {
        // Check for critical conditions
        if (cpuUsagePercent >= server.CpuCriticalThreshold ||
            memoryUsagePercent >= server.MemoryCriticalThreshold ||
            (diskUsagePercent.HasValue && diskUsagePercent >= server.DiskCriticalThreshold))
        {
            return ServerStatus.Critical;
        }

        // Check for warning conditions
        if (cpuUsagePercent >= server.CpuWarningThreshold ||
            memoryUsagePercent >= server.MemoryWarningThreshold ||
            (diskUsagePercent.HasValue && diskUsagePercent >= server.DiskWarningThreshold))
        {
            return ServerStatus.Warning;
        }

        return ServerStatus.Online;
    }
}
