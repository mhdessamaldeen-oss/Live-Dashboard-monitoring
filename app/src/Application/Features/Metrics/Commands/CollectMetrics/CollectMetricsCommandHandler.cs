using Application.Common;
using Application.Interfaces;
using Application.DTOs.Metrics;
using Domain.Common;
using Application.DTOs.Alerts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Metrics.Commands.CollectMetrics;

/// <summary>
/// Orchestrates the collection of system metrics from a target server.
/// This handler is responsible for gathering data, persisting snapshots, 
/// evaluating health thresholds, and triggering real-time notifications.
/// </summary>
public class CollectMetricsCommandHandler : IRequestHandler<CollectMetricsCommand, Result>
{
    private readonly IServerRepository _serverRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IMetricRepository _metricRepository;
    private readonly IDiskRepository _diskRepository;
    private readonly IMetricsProvider _metricsProvider;
    private readonly INotificationService _notificationService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CollectMetricsCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CollectMetricsCommandHandler(
        IServerRepository serverRepository,
        IAlertRepository alertRepository,
        IMetricRepository metricRepository,
        IDiskRepository diskRepository,
        IMetricsProvider metricsProvider,
        INotificationService notificationService,
        ICacheService cacheService,
        ILogger<CollectMetricsCommandHandler> logger,
        IMapper mapper)
    {
        _serverRepository = serverRepository;
        _alertRepository = alertRepository;
        _metricRepository = metricRepository;
        _diskRepository = diskRepository;
        _metricsProvider = metricsProvider;
        _notificationService = notificationService;
        _cacheService = cacheService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the metric collection process for a specific server.
    /// </summary>
    /// <param name="request">The command containing the ServerId</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A result indicating success or failure of the collection cycle</returns>
    public async Task<Result> Handle(CollectMetricsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting metrics collection for Server {ServerId}", request.ServerId);

        var server = await _serverRepository.GetActiveServerByIdAsync(request.ServerId, cancellationToken);
        if (server == null)
        {
            return Result.Failure("Server not found or inactive.");
        }

        // 1. Data Retrieval: Fetch real-time metrics from the hardware/os provider
        var metricDto = await _metricsProvider.CollectMetricsAsync(server.Id, cancellationToken);
        
        // 2. Persistence: Map DTO to Domain Entity and save snapshot
        var metric = new Metric
        {
            ServerId = server.Id,
            CpuUsagePercent = metricDto.CpuUsagePercent,
            MemoryUsagePercent = metricDto.MemoryUsagePercent,
            DiskUsagePercent = metricDto.DiskUsagePercent, 
            MemoryUsedBytes = metricDto.MemoryUsedBytes,
            MemoryTotalBytes = metricDto.MemoryTotalBytes,
            NetworkInBytesPerSec = metricDto.NetworkInBytesPerSec,
            NetworkOutBytesPerSec = metricDto.NetworkOutBytesPerSec,
            ActiveProcesses = metricDto.ActiveProcesses,
            SystemUptime = metricDto.SystemUptime,
            Timestamp = DateTime.UtcNow
        };

        await _metricRepository.AddAsync(metric, cancellationToken);

        // 3. Granular Disk Storage: Save individual drive metrics to enable per-disk monitoring
        foreach (var diskDto in metricDto.Disks)
        {
            var disk = new Disk
            {
                ServerId = server.Id,
                DriveLetter = diskDto.DriveLetter,
                FreeSpaceMB = diskDto.FreeSpaceMB,
                TotalSpaceMB = diskDto.TotalSpaceMB,
                UsedPercentage = diskDto.UsedPercentage,
                Timestamp = DateTime.UtcNow
            };
            await _diskRepository.AddAsync(disk, cancellationToken);
        }

        // 4. Threshold Logic: Evaluate metrics against configured server limits
        var newAlerts = await EvaluateThresholdsAsync(server, metric, metricDto.Disks, cancellationToken);

        // 5. State Management: Update the server's overall health status based on latest data
        var newStatus = ThresholdEvaluator.DetermineServerStatus(
            metric.CpuUsagePercent,
            metric.MemoryUsagePercent,
            metric.DiskUsagePercent,
            server);

        if (server.Status != newStatus)
        {
            _logger.LogInformation("Server {ServerName} status changed from {Old} to {New}", 
                server.Name, server.Status, newStatus);
            server.Status = newStatus;
            server.UpdatedAt = DateTime.UtcNow;
            _serverRepository.Update(server);
        }

        await _metricRepository.SaveChangesAsync(cancellationToken);

        // 6. Caching: Store the absolute latest snapshot in Redis for high-frequency frontend polling/updates
        metricDto.Status = server.Status.ToString();
        var latestMetrics = new Application.DTOs.Metrics.LatestMetricsDto(metricDto);
        await _cacheService.SetAsync($"server:{server.Id}:latest", latestMetrics, TimeSpan.FromMinutes(10), cancellationToken);

        // 7. Real-Time (SignalR): Push updates to globally connected dashboards and specific server details rooms
        await _notificationService.SendMetricUpdateAsync(server.Id, latestMetrics, cancellationToken);

        // 8. Alerts Notification: Broadcast newly triggered alerts via SignalR
        foreach (var alert in newAlerts)
        {
            _logger.LogWarning("Alert triggered for {ServerName}: {Title}", server.Name, alert.Title);
            var alertDto = _mapper.Map<AlertDto>(alert);
            await _notificationService.SendAlertTriggeredAsync(alertDto, cancellationToken);
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates system metrics against warning and critical thresholds.
    /// </summary>
    private async Task<List<Alert>> EvaluateThresholdsAsync(Server server, Metric metric, List<DiskDto> disks, CancellationToken cancellationToken)
    {
        var alerts = new List<Alert>();

        // CPU Usage Check
        var cpuCheck = ThresholdEvaluator.EvaluateCpu(server, metric.CpuUsagePercent);
        if (cpuCheck.ShouldAlert)
        {
            var alert = await CreateAlertAsync(server, "CPU Usage Alert", cpuCheck.Message, AlertSeverity.Warning, "CPU", metric.CpuUsagePercent, server.CpuWarningThreshold, cancellationToken);
            if (alert != null) alerts.Add(alert);
        }

        // Memory Usage Check
        var memCheck = ThresholdEvaluator.EvaluateMemory(server, metric.MemoryUsagePercent);
        if (memCheck.ShouldAlert)
        {
            var alert = await CreateAlertAsync(server, "Memory Usage Alert", memCheck.Message, AlertSeverity.Warning, "Memory", metric.MemoryUsagePercent, server.MemoryWarningThreshold, cancellationToken);
            if (alert != null) alerts.Add(alert);
        }

        // Individual Disk Drive Validation
        foreach (var disk in disks)
        {
             if (disk.UsedPercentage >= server.DiskCriticalThreshold)
             {
                 var title = $"Disk {disk.DriveLetter} Critical";
                 var message = $"Disk {disk.DriveLetter} is {disk.UsedPercentage:F1}% full (Critical > {server.DiskCriticalThreshold}%)";
                 var alert = await CreateAlertAsync(server, title, message, AlertSeverity.Critical, "Disk", disk.UsedPercentage, server.DiskCriticalThreshold, cancellationToken);
                 if (alert != null) alerts.Add(alert);
             }
             else if (disk.UsedPercentage >= server.DiskWarningThreshold)
             {
                 var title = $"Disk {disk.DriveLetter} Warning";
                 var message = $"Disk {disk.DriveLetter} is {disk.UsedPercentage:F1}% full (Warning > {server.DiskWarningThreshold}%)";
                 var alert = await CreateAlertAsync(server, title, message, AlertSeverity.Warning, "Disk", disk.UsedPercentage, server.DiskWarningThreshold, cancellationToken);
                 if (alert != null) alerts.Add(alert);
             }
        }

        return alerts;
    }

    /// <summary>
    /// Persists a new alert with anti-spam logic to prevent flooding for the same issue.
    /// </summary>
    private async Task<Alert?> CreateAlertAsync(
        Server server, 
        string title, 
        string message, 
        AlertSeverity severity, 
        string metricType, 
        double metricValue, 
        double threshold, 
        CancellationToken cancellationToken)
    {
        // Anti-Spam: Check for existing active alert to avoid spamming the user
        var lastAlert = await _alertRepository.GetLastActiveAlertAsync(server.Id, title, cancellationToken);

        // Don't create if an active alert for the exact same issue exists within the last hour
        if (lastAlert != null && (DateTime.UtcNow - lastAlert.CreatedAt).TotalHours < 1)
        {
             return null;
        }

        var alert = new Alert
        {
            ServerId = server.Id,
            Title = title,
            Message = message,
            Status = AlertStatus.Active, 
            Severity = severity,
            MetricType = metricType,
            MetricValue = metricValue,
            ThresholdValue = threshold,
            CreatedAt = DateTime.UtcNow
        };

        await _alertRepository.AddAsync(alert, cancellationToken);
        
        return alert;
    }
}
