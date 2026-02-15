using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Result<UserDto>>
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {request.Id} not found.");
        }

        // Check if email already exists (and not this user)
        if (user.Email != request.Email && await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<UserDto>.Failure("A user with this email already exists.");
        }

        // Update fields
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        // Update Role
        // 1. Get requested role
        var role = await _roleRepository.GetByNameAsync(request.Role, cancellationToken);
        if (role == null)
        {
            return Result<UserDto>.Failure($"Role '{request.Role}' not found.");
        }

        // 2. Remove existing roles
        var existingUserRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);
        
        _context.UserRoles.RemoveRange(existingUserRoles);

        // 3. Add new role
        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role.Name,
            IsActive = user.IsActive
        });
    }
}
