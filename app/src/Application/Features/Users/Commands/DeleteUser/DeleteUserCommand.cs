using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(int Id) : IRequest<Result<Unit>>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        
        if (user == null)
        {
            return Result<Unit>.Failure($"User with ID {request.Id} not found.");
        }

        // Check if user is the last admin? (Optional protection)

        // Remove UserRoles first
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);
        
        _context.UserRoles.RemoveRange(userRoles);
        
        // Remove User
        _context.Users.Remove(user);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
