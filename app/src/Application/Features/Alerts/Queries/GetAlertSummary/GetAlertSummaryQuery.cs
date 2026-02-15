using Application.Common;
using Application.DTOs.Alerts;
using MediatR;

namespace Application.Features.Alerts.Queries.GetAlertSummary;

public record GetAlertSummaryQuery() : IRequest<Result<AlertSummaryDto>>;
