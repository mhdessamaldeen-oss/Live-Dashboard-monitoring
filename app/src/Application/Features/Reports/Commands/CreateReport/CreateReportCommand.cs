using Application.Common;
using Application.DTOs.Reports;
using MediatR;

namespace Application.Features.Reports.Commands.CreateReport;

/// <summary>
/// Command to request a new report - starts Hangfire fire-and-forget job
/// </summary>
public record CreateReportCommand(
    int ServerId,
    string Title,
    string? Description,
    DateTime? DateRangeStart,
    DateTime? DateRangeEnd) : IRequest<Result<ReportDto>>;
