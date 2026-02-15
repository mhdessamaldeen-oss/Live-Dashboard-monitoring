using Application.Common;
using Application.DTOs.Auth;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.UpdateUser;
using Application.Features.Users.Queries.GetUsers;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Security;

namespace WebAPI.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return FromResult(await Mediator.Send(new GetUsersQuery()));
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        return FromResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }
        return FromResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<ActionResult<int>> DeleteUser(int id)
    {
        return FromResult(await Mediator.Send(new Application.Features.Users.Commands.DeleteUser.DeleteUserCommand(id)));
    }
}
