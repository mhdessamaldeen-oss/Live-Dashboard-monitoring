using Application.DTOs.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponse>>> Register([FromBody] RegisterCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result<AuthResponse>>> Refresh([FromBody] RefreshTokenCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}
