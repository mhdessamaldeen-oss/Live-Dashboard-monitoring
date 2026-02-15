using Application.Common;
using Application.DTOs.Alerts;
using Domain.Enums;
using MediatR;

namespace Application.Features.Alerts.Queries.GetAlerts;

public record GetAlertsQuery(
    int? ServerId = null,
    AlertStatus? Status = null,
    AlertSeverity? Severity = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<AlertDto>>>;
