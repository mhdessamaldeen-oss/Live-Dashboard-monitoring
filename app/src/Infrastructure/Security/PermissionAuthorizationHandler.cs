using Application.Common.Security;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Infrastructure.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserRepository _userRepository;

    public PermissionAuthorizationHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return;
        }

        // Optimized: Only get permissions if not already in claims or fetch once per request if needed
        // For simplicity and to show Dapper power, we fetch from DB. 
        // In production, you might want to cache this or include it in the JWT claims if the count is small.
        var permissions = await _userRepository.GetPermissionsByUserIdAsync(userId);

        if (permissions.Any(p => p == requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
