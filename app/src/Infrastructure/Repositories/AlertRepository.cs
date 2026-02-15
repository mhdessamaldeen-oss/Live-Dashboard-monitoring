using Application.DTOs.Alerts;
using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Alert?> GetWithServerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsByServerIdAsync(int serverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ServerId == serverId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Alert?> GetLastActiveAlertAsync(int serverId, string title, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ServerId == serverId && a.Title == title && a.Status == Domain.Enums.AlertStatus.Active)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<dynamic> Items, int TotalCount)> GetPagedAlertsAsync(
        int? serverId, 
        Domain.Enums.AlertStatus? status, 
        Domain.Enums.AlertSeverity? severity, 
        string? sortBy,
        bool sortDescending,
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new Dapper.SqlBuilder();
        var selector = sqlBuilder.AddTemplate(@"
            SELECT COUNT(*) FROM Alerts a /**where**/;
            SELECT a.*, s.Name as ServerName 
            FROM Alerts a 
            INNER JOIN Servers s ON a.ServerId = s.Id 
            /**where**/ 
            /**orderby**/ 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        if (serverId.HasValue)
        {
            sqlBuilder.Where("a.ServerId = @ServerId", new { ServerId = serverId.Value });
        }

        if (status.HasValue)
        {
            sqlBuilder.Where("a.Status = @Status", new { Status = (int)status.Value });
        }

        if (severity.HasValue)
        {
            sqlBuilder.Where("a.Severity = @Severity", new { Severity = (int)severity.Value });
        }

        var orderDir = sortDescending ? "DESC" : "ASC";
        var sortClause = (sortBy?.ToLower()) switch
        {
            "servername" => $"s.Name {orderDir}",
            "severity" => $"a.Severity {orderDir}",
            "status" => $"a.Status {orderDir}",
            "message" => $"a.Message {orderDir}",
            "createdat" => $"a.CreatedAt {orderDir}",
            _ => $"a.CreatedAt {orderDir}"
        };

        sqlBuilder.OrderBy(sortClause);

        var parameters = new Dapper.DynamicParameters(selector.Parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var multi = await _context.Connection.QueryMultipleAsync(selector.RawSql, parameters);
        
        var totalCount = await multi.ReadFirstAsync<int>();
        var items = await multi.ReadAsync<dynamic>();

        return (items, totalCount);
    }

    public async Task<AlertSummaryDto> GetAlertSummaryAsync(CancellationToken cancellationToken = default)
    {
        var allAlerts = _dbSet;
        var activeAlerts = allAlerts.Where(a => a.Status != Domain.Enums.AlertStatus.Resolved);

        return new AlertSummaryDto(
            TotalAlerts: await allAlerts.CountAsync(cancellationToken),
            ActiveAlerts: await activeAlerts.CountAsync(cancellationToken),
            CriticalAlerts: await activeAlerts.CountAsync(a => a.Severity == Domain.Enums.AlertSeverity.Critical, cancellationToken),
            WarningAlerts: await activeAlerts.CountAsync(a => a.Severity == Domain.Enums.AlertSeverity.Warning, cancellationToken),
            InfoAlerts: await activeAlerts.CountAsync(a => a.Severity == Domain.Enums.AlertSeverity.Info, cancellationToken)
        );
    }

    public async Task<int> ArchiveResolvedAlertsAsync(CancellationToken cancellationToken = default)
    {
        var resolvedAlerts = await _dbSet
            .Where(a => a.Status == Domain.Enums.AlertStatus.Resolved)
            .ToListAsync(cancellationToken);

        if (!resolvedAlerts.Any())
        {
            return 0;
        }

        foreach (var alert in resolvedAlerts)
        {
            alert.Status = Domain.Enums.AlertStatus.Expired;
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        return resolvedAlerts.Count;
    }

    public async Task<int> ResolveOldAlertsAsync(Domain.Enums.AlertSeverity severity, Domain.Enums.AlertStatus status, DateTime cutoff, string resolution, CancellationToken cancellationToken = default)
    {
        var alerts = await _dbSet
            .Where(a => a.Severity == severity && a.Status == status && a.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (!alerts.Any()) return 0;

        foreach (var alert in alerts)
        {
            alert.Status = Domain.Enums.AlertStatus.Resolved;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.Resolution = resolution;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return alerts.Count;
    }
}
