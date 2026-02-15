namespace Application.Interfaces;

/// <summary>
/// Holds the current metrics provider mode, switchable at runtime.
/// </summary>
public interface IMetricsProviderSettings
{
    MetricsProviderMode CurrentMode { get; set; }
}

public enum MetricsProviderMode
{
    Mock,
    System
}
