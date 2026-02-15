using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Repository interface for Server entity operations
/// </summary>
public interface IServerRepository : IRepository<Server>
{
    /// <summary>
    /// Fetches a server that is currently active and marked as enabled for monitoring
    /// </summary>
    Task<Server?> GetActiveServerByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all servers that are globally active
    /// </summary>
    Task<IEnumerable<Server>> GetActiveServersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs advanced server search with pagination, filtering by status, and dynamic sorting
    /// </summary>
    /// <param name="search">Keyword search against name/address</param>
    /// <param name="status">Filter by specific server health status</param>
    /// <param name="sortBy">The property name to sort by</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <param name="page">The 1-based page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="onlyHost">If true, returns only the monitoring host machines</param>
    Task<(IEnumerable<Server> Items, int TotalCount)> GetPagedServersAsync(
        string? search, 
        Domain.Enums.ServerStatus? status, 
        string sortBy, 
        bool sortDescending, 
        int page, 
        int pageSize, 
        bool onlyHost = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a server along with its most recent metric history
    /// </summary>
    Task<(Server Server, IEnumerable<Metric> Metrics)?> GetServerDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggressively deletes a server ensuring all metrics, disks, and alerts are purged
    /// </summary>
    Task DeleteWithRelatedAsync(int id, CancellationToken cancellationToken = default);
}
