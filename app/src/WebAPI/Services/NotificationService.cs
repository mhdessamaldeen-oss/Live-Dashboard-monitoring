using Application.DTOs.Alerts;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;

namespace WebAPI.Services;

public class NotificationService : INotificationService, IScopedService
{
    private readonly IHubContext<MonitoringHub> _hubContext;

    public NotificationService(IHubContext<MonitoringHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendMetricUpdateAsync(int serverId, object metric, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Groups(new[] { $"server-{serverId}", "dashboard" })
            .SendAsync("ReceiveMetricUpdate", metric, cancellationToken);
    }

    public async Task SendAlertTriggeredAsync(AlertDto alert, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Groups(new[] { $"server-{alert.ServerId}", "dashboard" })
            .SendAsync("ReceiveAlertTriggered", alert, cancellationToken);
    }

    public async Task SendAlertResolvedAsync(AlertDto alert, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Groups(new[] { $"server-{alert.ServerId}", "dashboard" })
            .SendAsync("ReceiveAlertResolved", alert, cancellationToken);
    }

    public async Task SendPresenceChangedAsync(int onlineCount, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All
            .SendAsync("ReceivePresenceChanged", onlineCount, cancellationToken);
    }

    public async Task SendReportReadyAsync(int userId, int reportId, string reportTitle, CancellationToken cancellationToken = default)
    {
        // TODO: Target user-specific group (e.g. "user-{userId}") once connection tracking is added
        await _hubContext.Clients.All
            .SendAsync("ReceiveReportReady", new { userId, reportId, reportTitle }, cancellationToken);
    }
}
