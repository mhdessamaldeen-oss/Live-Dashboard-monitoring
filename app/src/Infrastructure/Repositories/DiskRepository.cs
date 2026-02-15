using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DiskRepository : IDiskRepository
{
    private readonly ApplicationDbContext _context;

    public DiskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Disk disk, CancellationToken cancellationToken = default)
    {
        await _context.Disks.AddAsync(disk, cancellationToken);
    }

    public async Task<IEnumerable<Disk>> GetLatestByServerIdAsync(int serverId, CancellationToken cancellationToken = default)
    {
        // Get the latest timestamp for disks for this server
        var latestTimestamp = await _context.Disks
            .Where(d => d.ServerId == serverId)
            .OrderByDescending(d => d.Timestamp)
            .Select(d => d.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestTimestamp == default) return Enumerable.Empty<Disk>();

        // Return all disks with that timestamp (assuming batch collection)
        // Or closely matching timestamp (within 1 second)
        return await _context.Disks
            .Where(d => d.ServerId == serverId && d.Timestamp == latestTimestamp)
            .ToListAsync(cancellationToken);
    }
}
