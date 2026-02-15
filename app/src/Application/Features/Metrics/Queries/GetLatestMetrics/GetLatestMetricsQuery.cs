using Application.Common;
using Application.DTOs.Metrics;
using MediatR;

namespace Application.Features.Metrics.Queries.GetLatestMetrics;

/// <summary>
/// Query for latest metrics - uses Redis cache for fast access
/// </summary>
public record GetLatestMetricsQuery(int ServerId) : IRequest<Result<LatestMetricsDto>>;
