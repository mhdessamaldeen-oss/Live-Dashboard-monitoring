using Application.Common;
using WebAPI.Security;
using Application.DTOs.Servers;
using Application.Features.Servers.Commands.CreateServer;
using Application.Features.Servers.Commands.DeleteServer;
using Application.Features.Servers.Commands.UpdateServer;
using Application.Features.Servers.Queries.GetServerById;
using Application.Features.Servers.Queries.GetServers;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing monitored servers
/// </summary>
[Authorize]
public class ServersController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a paged list of servers with optional filtering and sorting
    /// </summary>
    /// <param name="query">The search, pagination, and sorting parameters</param>
    /// <returns>A paged result of server DTOs</returns>
    [HttpGet]
    [HasPermission(Permissions.Servers.View)]
    public async Task<ActionResult> GetServers([FromQuery] GetServersQuery query)
    {
        return FromResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a specific server by its unique identifier
    /// </summary>
    /// <param name="id">The server database ID</param>
    /// <returns>The server details</returns>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Servers.View)]
    public async Task<ActionResult> GetServer(int id)
    {
        return FromResult(await Mediator.Send(new GetServerByIdQuery(id)));
    }

    /// <summary>
    /// Alias for GetServer to provide detailed view data
    /// </summary>
    [HttpGet("{id}/details")]
    [HasPermission(Permissions.Servers.View)]
    public async Task<ActionResult> GetServerDetails(int id)
    {
        return FromResult(await Mediator.Send(new GetServerByIdQuery(id)));
    }

    /// <summary>
    /// Provisions a new server for monitoring
    /// </summary>
    /// <param name="command">The server configuration payload</param>
    /// <returns>The created server DTO</returns>
    [HttpPost]
    [HasPermission(Permissions.Servers.Create)]
    public async Task<ActionResult> CreateServer([FromBody] CreateServerCommand command)
    {
        return FromResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates an existing server's configuration
    /// </summary>
    /// <param name="id">The server ID to update</param>
    /// <param name="command">The new configuration data</param>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Servers.Edit)]
    public async Task<ActionResult> UpdateServer(int id, [FromBody] UpdateServerCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        return FromResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a server and its associated metrics/alerts
    /// </summary>
    /// <param name="id">The server ID to remove</param>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Servers.Delete)]
    public async Task<ActionResult> DeleteServer(int id)
    {
        return FromResult(await Mediator.Send(new DeleteServerCommand(id)));
    }
}
