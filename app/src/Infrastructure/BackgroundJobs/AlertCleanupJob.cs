using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class AlertCleanupJob : IScopedService
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AlertCleanupJob> _logger;

    public AlertCleanupJob(IAlertRepository alertRepository, ILogger<AlertCleanupJob> logger)
    {
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting alert cleanup job at {Time}", DateTime.UtcNow);

        // Auto-resolve informational alerts older than 24 hours
        var cutoff = DateTime.UtcNow.AddDays(-1);
        
        var resolvedCount = await _alertRepository.ResolveOldAlertsAsync(
            AlertSeverity.Info, 
            AlertStatus.Active, 
            cutoff, 
            "Auto-resolved by system maintenance job.");

        if (resolvedCount > 0)
        {
            _logger.LogInformation("Resolved {Count} informational alerts.", resolvedCount);
        }

        _logger.LogInformation("Alert cleanup job completed.");
    }
}
