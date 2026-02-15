using Domain.Entities;

namespace Application.Interfaces;

public interface IMetricRepository : IRepository<Metric>
{
    Task<IEnumerable<Metric>> GetByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Metric>> GetLatestByServerIdAsync(int serverId, int count = 10, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Metric> Items, int TotalCount)> GetPagedMetricsAsync(
        int serverId, 
        DateTime? from, 
        DateTime? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}
