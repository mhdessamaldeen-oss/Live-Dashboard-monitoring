using Application.Common;
using Application.DTOs.Metrics;
using MediatR;

namespace Application.Features.Metrics.Queries.GetMetrics;

public record GetMetricsQuery(
    int ServerId,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 100) : IRequest<Result<PagedResult<MetricDto>>>;
