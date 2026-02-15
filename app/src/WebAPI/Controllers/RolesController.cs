using Application.Common;
using Application.DTOs.Roles;
using Application.Features.Roles.Commands.CreateRole;
using Application.Features.Roles.Commands.UpdateRolePermissions;
using Application.Features.Roles.Queries.GetPermissions;
using Application.Features.Roles.Queries.GetRoles;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Security;

namespace WebAPI.Controllers;

[ApiController]
public class RolesController : ApiControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Users.View)] // Assuming viewing roles is related to user management
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        return FromResult(await Mediator.Send(new GetRolesQuery()));
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.Users.View)]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
    {
        return FromResult(await Mediator.Send(new GetAllPermissionsQuery()));
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult<int>> CreateRole(CreateRoleCommand command)
    {
        return FromResult(await Mediator.Send(command));
    }

    [HttpPut("{id}/permissions")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult<int>> UpdateRolePermissions(int id, UpdateRolePermissionsCommand command)
    {
        if (id != command.RoleId)
        {
            return BadRequest("Role ID mismatch");
        }
        return FromResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult<int>> DeleteRole(int id)
    {
        return FromResult(await Mediator.Send(new Application.Features.Roles.Commands.DeleteRole.DeleteRoleCommand(id)));
    }
}
