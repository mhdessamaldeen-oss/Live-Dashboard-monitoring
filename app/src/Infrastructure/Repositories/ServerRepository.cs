using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Data access implementation for Server entities.
/// Combines EF Core for standard CRUD and Dapper for high-performance paged queries and bulk operations.
/// </summary>
public class ServerRepository : Repository<Server>, IServerRepository
{
    public ServerRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Fetches a server by ID only if it is marked as active.
    /// </summary>
    public async Task<Server?> GetActiveServerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Id == id && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a list of all active servers for background monitoring tasks.
    /// </summary>
    public async Task<IEnumerable<Server>> GetActiveServersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Implements high-performance server searching and paging using Dapper and SQL template building.
    /// </summary>
    public async Task<(IEnumerable<Server> Items, int TotalCount)> GetPagedServersAsync(
        string? search, 
        Domain.Enums.ServerStatus? status, 
        string sortBy, 
        bool sortDescending, 
        int page, 
        int pageSize, 
        bool onlyHost = false,
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new Dapper.SqlBuilder();
        var selector = sqlBuilder.AddTemplate(@"
            SELECT COUNT(*) FROM Servers /**where**/;
            SELECT * FROM Servers /**where**/ /**orderby**/ 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        if (onlyHost)
        {
            sqlBuilder.Where("IsHost = 1");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sqlBuilder.Where("(Name LIKE @Search OR HostName LIKE @Search)", new { Search = $"%{search}%" });
        }

        if (status.HasValue)
        {
            sqlBuilder.Where("Status = @Status", new { Status = (int)status.Value });
        }

        var orderDir = sortDescending ? "DESC" : "ASC";
        var orderBy = sortBy.ToLower() switch
        {
            "id" => $"Id {orderDir}",
            "name" => $"Name {orderDir}",
            "status" => $"Status {orderDir}",
            "ipaddress" => $"IpAddress {orderDir}",
            "operatingsystem" => $"OperatingSystem {orderDir}",
            "location" => $"Location {orderDir}",
            "createdat" => $"CreatedAt {orderDir}",
            _ => $"Id {orderDir}"
        };
        sqlBuilder.OrderBy(orderBy);

        var parameters = new Dapper.DynamicParameters(selector.Parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var multi = await _context.Connection.QueryMultipleAsync(selector.RawSql, parameters);
        
        var totalCount = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<Server>()).ToList();

        return (items, totalCount);
    }

    /// <summary>
    /// Retrieves a server and its 30 most recent metrics in a single database round-trip.
    /// </summary>
    public async Task<(Server Server, IEnumerable<Metric> Metrics)?> GetServerDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM Servers WHERE Id = @Id;
            SELECT TOP 30 * FROM Metrics WHERE ServerId = @Id ORDER BY Timestamp DESC;";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new { Id = id });
        
        var server = await multi.ReadFirstOrDefaultAsync<Server>();
        if (server == null) return null;

        var metrics = (await multi.ReadAsync<Metric>()).ToList();

        return (server, metrics);
    }

    /// <summary>
    /// Performs a hard delete of a server and cascades deletes to metrics, alerts, and reports via raw SQL.
    /// </summary>
    public async Task DeleteWithRelatedAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM Metrics WHERE ServerId = @Id;
            DELETE FROM Alerts WHERE ServerId = @Id;
            DELETE FROM Reports WHERE ServerId = @Id;
            DELETE FROM Disks WHERE ServerId = @Id;
            DELETE FROM Servers WHERE Id = @Id;";

        await _context.Connection.ExecuteAsync(sql, new { Id = id });
    }
}
