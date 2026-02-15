using Application.Common;
using Application.DTOs.Metrics;
using Application.Features.Metrics.Queries.GetLatestMetrics;
using Application.Features.Metrics.Queries.GetMetrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for accessing performance metrics for specific servers
/// </summary>
[Authorize]
[Route("api/v1/servers/{serverId}/metrics")]
public class MetricsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves historical metrics for a server with pagination and filtering
    /// </summary>
    /// <param name="serverId">The ID of the server to fetch metrics for</param>
    /// <param name="query">Filtering and pagination parameters</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<MetricDto>>> GetMetrics(int serverId, [FromQuery] GetMetricsQuery query)
    {
        if (serverId != query.ServerId)
        {
            return BadRequest("Server ID mismatch");
        }
        return Ok(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the most recent heart-beat and metric snapshot for a server
    /// </summary>
    /// <param name="serverId">The ID of the server</param>
    [HttpGet("latest")]
    public async Task<ActionResult<LatestMetricsDto>> GetLatestMetrics(int serverId)
    {
        return Ok(await Mediator.Send(new GetLatestMetricsQuery(serverId)));
    }
}
