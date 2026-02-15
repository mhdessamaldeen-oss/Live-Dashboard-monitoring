namespace Application.Interfaces;

/// <summary>
/// Current user context interface
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
