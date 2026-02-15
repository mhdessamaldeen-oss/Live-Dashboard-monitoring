using Application.DTOs.Alerts;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IAlertRepository : IRepository<Alert>
{
    Task<Alert?> GetWithServerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alert>> GetActiveAlertsByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    Task<Alert?> GetLastActiveAlertAsync(int serverId, string title, CancellationToken cancellationToken = default);
    Task<(IEnumerable<dynamic> Items, int TotalCount)> GetPagedAlertsAsync(
        int? serverId, 
        AlertStatus? status, 
        AlertSeverity? severity, 
        string? sortBy,
        bool sortDescending,
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    Task<AlertSummaryDto> GetAlertSummaryAsync(CancellationToken cancellationToken = default);
    Task<int> ArchiveResolvedAlertsAsync(CancellationToken cancellationToken = default);
    Task<int> ResolveOldAlertsAsync(AlertSeverity severity, AlertStatus status, DateTime cutoff, string resolution, CancellationToken cancellationToken = default);
}
