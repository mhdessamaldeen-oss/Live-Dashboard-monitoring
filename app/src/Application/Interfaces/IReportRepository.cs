using Domain.Entities;

namespace Application.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetWithServerAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<dynamic> Items, int TotalCount)> GetPagedReportsAsync(
        int? serverId, 
        Domain.Enums.ReportStatus? status, 
        string? sortBy,
        bool sortDescending,
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}
