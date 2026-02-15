using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public CreateUserCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IApplicationDbContext context, IAuthService authService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _context = context;
        _authService = authService;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<UserDto>.Failure("A user with this email already exists.");
        }

        // Find the requested role
        var role = await _roleRepository.GetByNameAsync(request.Role, cancellationToken);
        if (role == null)
        {
            return Result<UserDto>.Failure($"Role '{request.Role}' not found.");
        }

        // Hash password
        string passwordHash = await _authService.HashPasswordAsync(request.Password);

        // Create User entity
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Link Role
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };
        _context.UserRoles.Add(userRole);
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
