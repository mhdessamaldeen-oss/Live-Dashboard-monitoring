using Application.DTOs.Metrics;

namespace Application.Interfaces;

/// <summary>
/// Metrics collection provider interface (random generator or performance counters)
/// </summary>
public interface IMetricsProvider
{
    Task<MetricDto> CollectMetricsAsync(int serverId, CancellationToken cancellationToken = default);
}
