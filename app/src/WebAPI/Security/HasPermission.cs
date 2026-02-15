using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Security;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
