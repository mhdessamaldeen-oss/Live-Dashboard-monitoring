using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MetricRepository : Repository<Metric>, IMetricRepository
{
    public MetricRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Metric>> GetByServerIdAsync(int serverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.ServerId == serverId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Metric>> GetLatestByServerIdAsync(int serverId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.ServerId == serverId)
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Metric> Items, int TotalCount)> GetPagedMetricsAsync(
        int serverId, 
        DateTime? from, 
        DateTime? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new Dapper.SqlBuilder();
        var selector = sqlBuilder.AddTemplate(@"
            SELECT COUNT(*) FROM Metrics /**where**/;
            SELECT * FROM Metrics /**where**/ 
            ORDER BY Timestamp DESC 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        sqlBuilder.Where("ServerId = @ServerId", new { ServerId = serverId });

        if (from.HasValue)
        {
            sqlBuilder.Where("Timestamp >= @From", new { From = from.Value });
        }

        if (to.HasValue)
        {
            sqlBuilder.Where("Timestamp <= @To", new { To = to.Value });
        }

        var parameters = new Dapper.DynamicParameters(selector.Parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var multi = await _context.Connection.QueryMultipleAsync(selector.RawSql, parameters);
        
        var totalCount = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<Metric>()).ToList();

        return (items, totalCount);
    }
}
