using Application.Common;
using Application.DTOs.Roles;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Commands.UpdateRolePermissions;

public record UpdateRolePermissionsCommand : IRequest<Result<int>>
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public UpdateRolePermissionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FindAsync(new object[] { request.RoleId }, cancellationToken);

        if (role == null)
        {
            return Result<int>.Failure($"Role with ID {request.RoleId} not found.");
        }

        // 1. Get existing role permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        // 2. Identify permissions to add and remove
        var newPermissionIds = request.PermissionIds ?? new List<int>();
        var permissionsToAdd = newPermissionIds.Where(id => !existingPermissions.Any(ep => ep.PermissionId == id)).ToList();
        var permissionsToRemove = existingPermissions.Where(ep => !newPermissionIds.Contains(ep.PermissionId)).ToList();

        // 3. Add new permissions
        foreach (var permissionId in permissionsToAdd)
        {
            if (await _context.Permissions.AnyAsync(p => p.Id == permissionId, cancellationToken))
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }
        }

        // 4. Remove unwanted permissions
        if (permissionsToRemove.Any())
        {
            _context.RolePermissions.RemoveRange(permissionsToRemove);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(role.Id);
    }
}
