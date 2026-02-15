using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class SystemHealthCheckJob : IScopedService
{
    private readonly IServerRepository _serverRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<SystemHealthCheckJob> _logger;

    public SystemHealthCheckJob(
        IServerRepository serverRepository, 
        IAlertRepository alertRepository, 
        ILogger<SystemHealthCheckJob> logger)
    {
        _serverRepository = serverRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting system health check at {Time}", DateTime.UtcNow);

        try
        {
            // Verify connectivity through repositories
            var serverCount = await _serverRepository.CountAsync();
            var alertCount = await _alertRepository.CountAsync(a => a.Status == Domain.Enums.AlertStatus.Active);

            _logger.LogInformation("System Status: {Servers} Servers Monitored, {Alerts} Active Alerts.", serverCount, alertCount);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "System health check failed!");
        }

        _logger.LogInformation("System health check completed.");
    }
}
