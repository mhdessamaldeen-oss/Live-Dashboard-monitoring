using Application.Common;
using WebAPI.Security;
using Application.DTOs.Alerts;
using Application.Features.Alerts.Commands.AcknowledgeAlert;
using Application.Features.Alerts.Commands.ResolveAlert;
using Application.Features.Alerts.Commands.ArchiveResolvedAlerts;
using Application.Features.Alerts.Queries.GetAlertSummary;
using Application.Features.Alerts.Queries.GetAlerts;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize]
public class AlertsController : ApiControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Alerts.View)]
    public async Task<ActionResult<PagedResult<AlertDto>>> GetAlerts([FromQuery] GetAlertsQuery query)
    {
        return FromResult(await Mediator.Send(query));
    }

    [HttpGet("summary")]
    [HasPermission(Permissions.Alerts.View)]
    public async Task<ActionResult<AlertSummaryDto>> GetAlertSummary()
    {
        return FromResult(await Mediator.Send(new GetAlertSummaryQuery()));
    }

    [HttpPost("{id}/resolve")]
    [HasPermission(Permissions.Alerts.Resolve)]
    public async Task<ActionResult> ResolveAlert(int id, [FromBody] ResolveAlertCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }
        return FromResult(await Mediator.Send(command));
    }

    [HttpPost("{id}/acknowledge")]
    [HasPermission(Permissions.Alerts.Resolve)]
    public async Task<ActionResult> AcknowledgeAlert(int id)
    {
        return FromResult(await Mediator.Send(new AcknowledgeAlertCommand(id)));
    }

    [HttpPost("archive-resolved")]
    [HasPermission(Permissions.Alerts.Resolve)]
    public async Task<ActionResult> ArchiveResolvedAlerts()
    {
        return FromResult(await Mediator.Send(new ArchiveResolvedAlertsCommand()));
    }
}
