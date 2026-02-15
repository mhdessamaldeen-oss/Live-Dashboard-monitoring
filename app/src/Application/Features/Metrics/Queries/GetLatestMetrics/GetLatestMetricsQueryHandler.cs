using Application.Common;
using Application.DTOs.Metrics;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Metrics.Queries.GetLatestMetrics;

public class GetLatestMetricsQueryHandler : IRequestHandler<GetLatestMetricsQuery, Result<LatestMetricsDto>>
{
    private readonly ICacheService _cacheService;

    public GetLatestMetricsQueryHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<Result<LatestMetricsDto>> Handle(GetLatestMetricsQuery request, CancellationToken cancellationToken)
    {
        var cachedMetrics = await _cacheService.GetAsync<LatestMetricsDto>($"server:{request.ServerId}:latest", cancellationToken);

        if (cachedMetrics == null)
        {
            return Result<LatestMetricsDto>.Failure("No metrics available yet.");
        }

        return Result<LatestMetricsDto>.Success(cachedMetrics);
    }
}
