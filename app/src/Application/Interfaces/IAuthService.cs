using Application.DTOs.Auth;
using System.Security.Claims;

namespace Application.Interfaces;

/// <summary>
/// Provides core security functions including password hashing, verification, 
/// and JWT token lifecycle management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a secure cryptographical hash of a plain-text password using BCrypt
    /// </summary>
    Task<string> HashPasswordAsync(string password);

    /// <summary>
    /// Validates a candidate password against a stored security hash
    /// </summary>
    Task<bool> VerifyPasswordAsync(string password, string hash);

    /// <summary>
    /// Generates a signed JWT access token containing standard identity and role claims
    /// </summary>
    /// <param name="userId">The unique database user ID</param>
    /// <param name="email">The user's primary email/login</param>
    /// <param name="roles">A list of assigned role names (e.g., 'Admin')</param>
    /// <returns>A tuple containing the signed JWT string and its expiration timestamp</returns>
    Task<(string Token, DateTime Expiration)> GenerateJwtTokenAsync(int userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Generates a long-lived, high-entropy refresh token string
    /// </summary>
    Task<string> GenerateRefreshTokenAsync();

    /// <summary>
    /// Retrieves the security principal from an expired JWT token
    /// </summary>
    Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token);
}
