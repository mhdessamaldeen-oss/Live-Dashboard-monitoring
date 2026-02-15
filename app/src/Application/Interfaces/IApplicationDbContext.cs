using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Interfaces;

/// <summary>
/// Application database context interface wrapper for EF Core DbContext
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Server> Servers { get; }
    DbSet<Metric> Metrics { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<Disk> Disks { get; }
    DbSet<Report> Reports { get; }
    
    IDbConnection Connection { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
