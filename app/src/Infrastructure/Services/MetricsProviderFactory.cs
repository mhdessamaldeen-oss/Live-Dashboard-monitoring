using Application.DTOs.Metrics;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Factory that delegates to the correct metrics provider based on:
/// - Mock mode → MockMetricsProvider (random data for all servers)
/// - System mode on Windows → SystemMetricsService (PerformanceCounters)
/// - System mode on Linux/Docker → LinuxMetricsService (/proc filesystem)
/// </summary>
public class MetricsProviderFactory : IMetricsProvider, IScopedService
{
    private readonly IMetricsProviderSettings _settings;
    private readonly MockMetricsProvider _mockProvider;
    private readonly SystemMetricsService _windowsProvider;
    private readonly LinuxMetricsService _linuxProvider;
    private readonly ILogger<MetricsProviderFactory> _logger;

    public MetricsProviderFactory(
        IMetricsProviderSettings settings,
        MockMetricsProvider mockProvider,
        SystemMetricsService windowsProvider,
        LinuxMetricsService linuxProvider,
        ILogger<MetricsProviderFactory> logger)
    {
        _settings = settings;
        _mockProvider = mockProvider;
        _windowsProvider = windowsProvider;
        _linuxProvider = linuxProvider;
        _logger = logger;
    }

    public Task<MetricDto> CollectMetricsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        if (_settings.CurrentMode == MetricsProviderMode.System)
        {
            if (OperatingSystem.IsWindows())
            {
                _logger.LogDebug("Using Windows SystemMetricsService for server {ServerId}", serverId);
                return _windowsProvider.CollectMetricsAsync(serverId, cancellationToken);
            }

            if (OperatingSystem.IsLinux())
            {
                _logger.LogDebug("Using Linux MetricsService for server {ServerId}", serverId);
                return _linuxProvider.CollectMetricsAsync(serverId, cancellationToken);
            }

            // macOS or other unsupported OS → fallback to Mock
            _logger.LogWarning("System metrics not supported on this OS. Falling back to Mock provider.");
            return _mockProvider.CollectMetricsAsync(serverId, cancellationToken);
        }

        return _mockProvider.CollectMetricsAsync(serverId, cancellationToken);
    }
}
