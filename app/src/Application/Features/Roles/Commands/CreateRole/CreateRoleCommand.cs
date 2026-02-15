using Application.Common;
using Application.DTOs.Roles;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Result<int>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
        {
            return Result<int>.Failure($"Role '{request.Name}' already exists.");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            foreach (var permissionId in request.PermissionIds)
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
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(role.Id);
    }
}
