using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public LoginCommandHandler(IUserRepository userRepository, IApplicationDbContext context, IAuthService authService)
    {
        _userRepository = userRepository;
        _context = context;
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !await _authService.VerifyPasswordAsync(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure("Account is inactive.");
        }

        var roles = (await _userRepository.GetRolesByUserIdAsync(user.Id, cancellationToken)).ToList();
        var (token, expiration) = await _authService.GenerateJwtTokenAsync(user.Id, user.Email, roles);
        var refreshToken = await _authService.GenerateRefreshTokenAsync();

        user.LastLoginAt = DateTime.UtcNow;
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        var primaryRole = roles.FirstOrDefault() ?? "User";

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
                Role = primaryRole,
                IsActive = user.IsActive
            }
        ));
    }
}
