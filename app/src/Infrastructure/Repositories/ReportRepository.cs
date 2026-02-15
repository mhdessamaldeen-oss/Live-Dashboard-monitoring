using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Report>> GetWithServerAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<dynamic> Items, int TotalCount)> GetPagedReportsAsync(
        int? serverId, 
        ReportStatus? status, 
        string? sortBy,
        bool sortDescending,
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new Dapper.SqlBuilder();
        var selector = sqlBuilder.AddTemplate(@"
            SELECT COUNT(*) FROM Reports r /**where**/;
            SELECT r.*, s.Name as ServerName 
            FROM Reports r 
            LEFT JOIN Servers s ON r.ServerId = s.Id 
            /**where**/ 
            /**orderby**/ 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        if (serverId.HasValue)
        {
            sqlBuilder.Where("r.ServerId = @ServerId");
        }

        if (status.HasValue)
        {
            sqlBuilder.Where("r.Status = @Status");
        }

        var orderDir = sortDescending ? "DESC" : "ASC";
        var sortClause = (sortBy?.ToLower()) switch
        {
            "servername" => $"s.Name {orderDir}",
            "status" => $"r.Status {orderDir}",
            "title" => $"r.Title {orderDir}",
            "createdat" => $"r.CreatedAt {orderDir}",
            "completedat" => $"r.CompletedAt {orderDir}",
            _ => $"r.CreatedAt {orderDir}"
        };

        sqlBuilder.OrderBy(sortClause);

        var parameters = new Dapper.DynamicParameters();
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        if (serverId.HasValue)
        {
            parameters.Add("ServerId", serverId.Value);
        }

        if (status.HasValue)
        {
            parameters.Add("Status", (int)status.Value);
        }

        using var multi = await _context.Connection.QueryMultipleAsync(selector.RawSql, parameters);
        
        var totalCount = await multi.ReadFirstAsync<int>();
        var items = await multi.ReadAsync<dynamic>();

        return (items, totalCount);
    }
}
