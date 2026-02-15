using Application.Common;
using Application.DTOs.Roles;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty
                })
                .ToListAsync(cancellationToken);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description ?? string.Empty,
                Permissions = permissions
            });
        }

        return Result<IEnumerable<RoleDto>>.Success(roleDtos);
    }
}
