using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IUserRepository userRepository, IApplicationDbContext context, IAuthService authService)
    {
        _userRepository = userRepository;
        _context = context;
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Email is already registered.");
        }

        // 2. Hash password
        string passwordHash = await _authService.HashPasswordAsync(request.Password);

        // 3. Assign default role (User) if not exists
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        if (userRole == null)
        {
            return Result<AuthResponse>.Failure("Default user role not found.");
        }

        // 4. Create User entity
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Add UserRole link
        var userRoleLink = new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id
        };
        _context.UserRoles.Add(userRoleLink);

        // 6. Generate Tokens
        var (token, expiration) = await _authService.GenerateJwtTokenAsync(user.Id, user.Email, new[] { userRole.Name });
        var refreshToken = await _authService.GenerateRefreshTokenAsync();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            refreshToken,
            expiration,
            new UserDto 
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = userRole.Name,
                IsActive = user.IsActive
            }
        ));
    }
}
