using Application.Common;
using MediatR;

namespace Application.Features.Reports.Commands.GenerateReport;

/// <summary>
/// Command executed by Hangfire to actually generate the report file
/// </summary>
public record GenerateReportCommand(int ReportId) : IRequest<Result>;
