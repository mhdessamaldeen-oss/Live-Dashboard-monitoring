using Application.Features.Metrics.Commands.CollectMetrics;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class MetricCollectionJob : IScopedService
{
    private readonly IServerRepository _serverRepository;
    private readonly ISender _mediator;
    private readonly IMetricsProviderSettings _settings;
    private readonly ILogger<MetricCollectionJob> _logger;

    public MetricCollectionJob(
        IServerRepository serverRepository,
        ISender mediator,
        IMetricsProviderSettings settings,
        ILogger<MetricCollectionJob> logger)
    {
        _serverRepository = serverRepository;
        _mediator = mediator;
        _settings = settings;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var mode = _settings.CurrentMode;
        _logger.LogInformation("Starting metric collection job at {Time} — Mode: {Mode}", DateTime.UtcNow, mode);

        var activeServers = await _serverRepository.GetActiveServersAsync();

        if (mode == MetricsProviderMode.System)
        {
            // System mode: only the server marked as IsHost represents the real host machine
            var hostServer = activeServers.FirstOrDefault(s => s.IsHost);
            if (hostServer != null)
            {
                try
                {
                    await _mediator.Send(new CollectMetricsCommand(hostServer.Id));
                    _logger.LogInformation("Collected real system metrics for host server '{Name}' (ID: {Id})", hostServer.Name, hostServer.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting system metrics for host server {ServerId}", hostServer.Id);
                }
            }
        }
        else
        {
            // Mock mode: collect simulated metrics for ALL servers (demo/testing)
            foreach (var server in activeServers)
            {
                try
                {
                    await _mediator.Send(new CollectMetricsCommand(server.Id));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting metrics for server {ServerId}", server.Id);
                }
            }
        }

        _logger.LogInformation("Metric collection job completed.");
    }
}
