using Domain.Entities;
using Domain.Enums;
using Domain.Constants;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultDataAsync(ApplicationDbContext context)
    {
        // Permissions
        var definedPermissions = new List<Permission>
        {
            new() { Name = Permissions.Servers.View, Description = "Can view list of servers" },
            new() { Name = Permissions.Servers.Create, Description = "Can create new nodes" },
            new() { Name = Permissions.Servers.Edit, Description = "Can edit servers" },
            new() { Name = Permissions.Servers.Delete, Description = "Can delete servers" },
            new() { Name = Permissions.Alerts.View, Description = "Can view alerts" },
            new() { Name = Permissions.Alerts.Resolve, Description = "Can resolve alerts" },
            new() { Name = Permissions.Reports.View, Description = "Can view reports" },
            new() { Name = Permissions.Reports.Create, Description = "Can generate reports" },
            new() { Name = Permissions.Reports.Download, Description = "Can download reports" },
            new() { Name = Permissions.Users.View, Description = "Can view users" },
            new() { Name = Permissions.Users.Manage, Description = "Can manage users" }
        };

        foreach (var p in definedPermissions)
        {
            if (!await context.Permissions.AnyAsync(x => x.Name == p.Name))
            {
                context.Permissions.Add(p);
            }
        }
        await context.SaveChangesAsync();

        // Roles
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role { Name = Roles.Admin, Description = "Full system access" };
            var operatorRole = new Role { Name = Roles.Operator, Description = "Monitoring and management access" };
            var viewerRole = new Role { Name = Roles.Viewer, Description = "Read-only access" };
            
            context.Roles.AddRange(adminRole, operatorRole, viewerRole);
            await context.SaveChangesAsync();
        }

        // Admin → all permissions
        var adminRoleEntity = await context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Admin);
        if (adminRoleEntity != null)
        {
            var allPerms = await context.Permissions.ToListAsync();
            var existingLinks = await context.RolePermissions
                .Where(rp => rp.RoleId == adminRoleEntity.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var missingPerms = allPerms.Where(p => !existingLinks.Contains(p.Id)).ToList();
            
            foreach (var p in missingPerms)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = adminRoleEntity.Id, PermissionId = p.Id });
            }
            await context.SaveChangesAsync();
        }

        // Viewer → view-only permissions
        var viewerRoleEntity = await context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Viewer);
        if (viewerRoleEntity != null)
        {
            var viewPerms = await context.Permissions.Where(p => p.Name.EndsWith(".View")).ToListAsync();
            var existingLinks = await context.RolePermissions
                .Where(rp => rp.RoleId == viewerRoleEntity.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var missingPerms = viewPerms.Where(p => !existingLinks.Contains(p.Id)).ToList();
            
            foreach (var p in missingPerms)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = viewerRoleEntity.Id, PermissionId = p.Id });
            }
            await context.SaveChangesAsync();
        }

        // Operator → everything except user management and server deletion
        var operatorRoleEntity = await context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Operator);
        if (operatorRoleEntity != null)
        {
            var opPerms = await context.Permissions
                .Where(p => !p.Name.StartsWith("Permissions.Users") && p.Name != Permissions.Servers.Delete)
                .ToListAsync();
            
            var existingLinks = await context.RolePermissions
                .Where(rp => rp.RoleId == operatorRoleEntity.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var missingPerms = opPerms.Where(p => !existingLinks.Contains(p.Id)).ToList();
            
            foreach (var p in missingPerms)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = operatorRoleEntity.Id, PermissionId = p.Id });
            }
            await context.SaveChangesAsync();
        }

        // Default users
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == Roles.Admin);
            var viewerRole = await context.Roles.FirstAsync(r => r.Name == Roles.Viewer);

            var admin = new User
            {
                Email = "admin@demo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                IsActive = true
            };

            var user = new User
            {
                Email = "user@demo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                FirstName = "Standard",
                LastName = "User",
                IsActive = true
            };

            context.Users.AddRange(admin, user);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = viewerRole.Id });
            await context.SaveChangesAsync();
        }

        // Mark one server as host if none is flagged
        if (await context.Servers.AnyAsync() && !await context.Servers.AnyAsync(s => s.IsHost))
        {
            var server = await context.Servers.FirstOrDefaultAsync(s => s.IsActive);
            if (server != null)
            {
                server.IsHost = true;
                server.Name = "[HOST] " + server.Name.Replace("[HOST] ", "");
                await context.SaveChangesAsync();
            }
        }

        if (!await context.Servers.AnyAsync())
        {
            var servers = new List<Server>
            {
                new Server
                {
                    Name = "[HOST] LOCAL-SERVER",
                    HostName = Environment.MachineName,
                    IpAddress = "127.0.0.1",
                    OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    Location = "Local Machine",
                    Status = ServerStatus.Online,
                    IsActive = true,
                    IsHost = true
                },
                new Server
                {
                    Name = "PROD-DB-01",
                    HostName = "db01.production.internal",
                    IpAddress = "10.0.1.20",
                    OperatingSystem = "SQL Server 2022 on Windows Server 2022",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "APP-SRV-DEV",
                    HostName = "dev-app.internal",
                    IpAddress = "192.168.1.50",
                    OperatingSystem = "CentOS Stream 9",
                    Location = "Abu Dhabi DC 1",
                    Status = ServerStatus.Warning,
                    IsActive = true
                },
                new Server
                {
                    Name = "PROD-CACHE-01",
                    HostName = "cache01.production.internal",
                    IpAddress = "10.0.1.30",
                    OperatingSystem = "Ubuntu 22.04 LTS (Redis)",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "PROD-WEB-02",
                    HostName = "web02.production.internal",
                    IpAddress = "10.0.1.11",
                    OperatingSystem = "Ubuntu 22.04 LTS",
                    Location = "Abu Dhabi DC 2",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "BACKUP-SRV-01",
                    HostName = "backup01.internal",
                    IpAddress = "10.0.5.10",
                    OperatingSystem = "FreeBSD 13",
                    Location = "Al Ain DR Site",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "UAE-WEB-01",
                    HostName = "uae-web01.production.internal",
                    IpAddress = "10.2.1.10",
                    OperatingSystem = "Debian 11",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "UAE-DB-01",
                    HostName = "uae-db01.production.internal",
                    IpAddress = "10.2.1.20",
                    OperatingSystem = "PostgreSQL on Ubuntu 22.04",
                    Location = "Abu Dhabi DC 1",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "MONITOR-SRV-01",
                    HostName = "mon01.internal",
                    IpAddress = "10.0.9.5",
                    OperatingSystem = "Ubuntu 20.04 LTS",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "AUTH-PROX-01",
                    HostName = "auth01.internal",
                    IpAddress = "10.0.8.20",
                    OperatingSystem = "Red Hat Enterprise Linux 9",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "LOG-STACK-01",
                    HostName = "logs.production.internal",
                    IpAddress = "10.0.7.100",
                    OperatingSystem = "Ubuntu 22.04 LTS (ELK Stack)",
                    Location = "Abu Dhabi DC 1",
                    Status = ServerStatus.Warning,
                    IsActive = true
                },
                new Server
                {
                    Name = "DEV-SQL-01",
                    HostName = "dev-sql01.internal",
                    IpAddress = "192.168.1.55",
                    OperatingSystem = "Windows Server 2022",
                    Location = "Sharjah Office DC",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "PROD-MAIL-01",
                    HostName = "mail.production.internal",
                    IpAddress = "10.0.10.5",
                    OperatingSystem = "Ubuntu 22.04 LTS",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "PROD-LOAD-01",
                    HostName = "lb01.production.internal",
                    IpAddress = "10.0.1.5",
                    OperatingSystem = "NGINX on Debian 11",
                    Location = "Dubai (me-central-1)",
                    Status = ServerStatus.Online,
                    IsActive = true
                },
                new Server
                {
                    Name = "STAGE-APP-01",
                    HostName = "stage-app.internal",
                    IpAddress = "10.0.20.10",
                    OperatingSystem = "Ubuntu 22.04 LTS",
                    Location = "Fujairah DC",
                    Status = ServerStatus.Offline,
                    IsActive = true
                }
            };

            context.Servers.AddRange(servers);
            await context.SaveChangesAsync();
        }
    }
}
