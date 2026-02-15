using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IApplicationDbContext context, IAuthService authService)
    {
        _userRepository = userRepository;
        _context = context;
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var principal = await _authService.GetPrincipalFromExpiredTokenAsync(request.Token);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Result<AuthResponse>.Failure("Invalid token.");
            }

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
            }

            var roles = (await _userRepository.GetRolesByUserIdAsync(user.Id, cancellationToken)).ToList();
            var (newToken, expiration) = await _authService.GenerateJwtTokenAsync(user.Id, user.Email, roles);
            var newRefreshToken = await _authService.GenerateRefreshTokenAsync();

            user.RefreshToken = newRefreshToken;
            _userRepository.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            var primaryRole = roles.FirstOrDefault() ?? "User";

            return Result<AuthResponse>.Success(new AuthResponse(
                newToken,
                newRefreshToken,
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
        catch (Exception)
        {
            return Result<AuthResponse>.Failure("Could not refresh token.");
        }
    }
}
