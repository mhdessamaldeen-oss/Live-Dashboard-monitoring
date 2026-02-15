using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Disk> Disks => Set<Disk>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public System.Data.IDbConnection Connection => Database.GetDbConnection();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
