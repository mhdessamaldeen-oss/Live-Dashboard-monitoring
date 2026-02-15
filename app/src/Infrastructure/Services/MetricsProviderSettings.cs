using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Singleton that stores the current metrics provider mode in memory.
/// </summary>
public class MetricsProviderSettings : IMetricsProviderSettings, ISingletonService
{
    public MetricsProviderMode CurrentMode { get; set; } = MetricsProviderMode.Mock;
}
