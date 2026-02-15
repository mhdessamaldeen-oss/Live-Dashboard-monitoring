using Application.Common;
using Application.DTOs.Roles;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries.GetPermissions;

public record GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<IEnumerable<PermissionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<PermissionDto>>.Success(permissions);
    }
}
