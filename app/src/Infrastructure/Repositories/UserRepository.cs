using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT r.Name 
            FROM Roles r 
            INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
            WHERE ur.UserId = @UserId";

        return await _context.Connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<string>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DISTINCT p.Name 
            FROM Permissions p 
            INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId 
            INNER JOIN Roles r ON rp.RoleId = r.Id 
            INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
            WHERE ur.UserId = @UserId";

        return await _context.Connection.QueryAsync<string>(sql, new { UserId = userId });
    }
}
