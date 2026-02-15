using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(int Id) : IRequest<Result<Unit>>;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            return Result<Unit>.Failure($"Role with ID {request.Id} not found.");
        }

        if (role.Name == "Admin" || role.Name == "User") // Protect system roles
        {
             return Result<Unit>.Failure($"Cannot delete system role '{role.Name}'.");
        }

        var isAssignedToUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == role.Id, cancellationToken);
        if (isAssignedToUsers)
        {
            return Result<Unit>.Failure("Cannot delete role because it is assigned to one or more users.");
        }

        // Remove associated RolePermissions (Cascade delete might handle this, but being explicit is safer)
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync(cancellationToken);
        
        _context.RolePermissions.RemoveRange(rolePermissions);
        _context.Roles.Remove(role);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
