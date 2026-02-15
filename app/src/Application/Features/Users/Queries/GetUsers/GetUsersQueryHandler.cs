using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Dapper;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT u.Id, u.Email, u.FirstName, u.LastName, u.IsActive, 
                   (SELECT TOP 1 r.Name FROM Roles r 
                    INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
                    WHERE ur.UserId = u.Id) as Role
            FROM Users u";

        var users = await _context.Connection.QueryAsync<UserDto>(sql);

        return Result<IEnumerable<UserDto>>.Success(users);
    }
}
