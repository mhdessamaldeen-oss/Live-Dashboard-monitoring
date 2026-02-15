using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
