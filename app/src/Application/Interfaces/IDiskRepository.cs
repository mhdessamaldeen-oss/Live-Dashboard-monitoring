using Domain.Entities;

namespace Application.Interfaces;

public interface IDiskRepository
{
    Task AddAsync(Disk disk, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disk>> GetLatestByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
}
