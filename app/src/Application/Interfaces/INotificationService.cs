using Application.DTOs.Alerts;

namespace Application.Interfaces;

/// <summary>
/// SignalR notification service interface for real-time updates
/// </summary>
public interface INotificationService
{
    Task SendMetricUpdateAsync(int serverId, object metric, CancellationToken cancellationToken = default);
    Task SendAlertTriggeredAsync(AlertDto alert, CancellationToken cancellationToken = default);
    Task SendAlertResolvedAsync(AlertDto alert, CancellationToken cancellationToken = default);
    Task SendPresenceChangedAsync(int onlineCount, CancellationToken cancellationToken = default);
    Task SendReportReadyAsync(int userId, int reportId, string reportTitle, CancellationToken cancellationToken = default);
}
