using Application.Common;
using WebAPI.Security;
using Application.DTOs.Reports;
using Application.Features.Reports.Commands.CreateReport;
using Application.Features.Reports.Queries.DownloadReport;
using Application.Features.Reports.Queries.GetReports;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize]
public class ReportsController : ApiControllerBase
{
    /// <summary>
    /// Gets a paged list of reports with optional server and status filters.
    /// </summary>
    /// <param name="query">Filtering and pagination parameters.</param>
    /// <returns>A paged list of report metadata.</returns>
    [HttpGet]
    [HasPermission(Permissions.Reports.View)]
    public async Task<ActionResult> GetReports([FromQuery] GetReportsQuery query)
    {
        return FromResult(await Mediator.Send(query));
    }

    [HttpGet("stats")]
    [HasPermission(Permissions.Reports.View)]
    public async Task<ActionResult> GetStats()
    {
        return FromResult(await Mediator.Send(new Application.Features.Reports.Queries.GetReportStats.GetReportStatsQuery()));
    }

    /// <summary>
    /// Requests the generation of a new system performance report.
    /// This triggers a fire-and-forget background job.
    /// </summary>
    /// <param name="command">The report parameters including title and date range.</param>
    /// <returns>Metadata of the created report entry.</returns>
    [HttpPost]
    [HasPermission(Permissions.Reports.Create)]
    public async Task<ActionResult> CreateReport([FromBody] CreateReportCommand command)
    {
        return FromResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Downloads a completed report file.
    /// </summary>
    /// <param name="id">The unique identifier of the report.</param>
    /// <returns>The generated CSV file if available.</returns>
    [HttpGet("{id}/download")]
    [HasPermission(Permissions.Reports.Download)]
    public async Task<IActionResult> DownloadReport(int id)
    {
        var result = await Mediator.Send(new DownloadReportQuery(id));
        
        if (!result.IsSuccess || result.Data == null)
        {
            return NotFound(result.Errors);
        }

        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }
}
