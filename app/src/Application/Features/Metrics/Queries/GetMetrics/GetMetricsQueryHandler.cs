using Application.Common;
using Application.DTOs.Metrics;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Metrics.Queries.GetMetrics;

public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, Result<PagedResult<MetricDto>>>
{
    private readonly IMetricRepository _metricRepository;
    private readonly IMapper _mapper;

    public GetMetricsQueryHandler(IMetricRepository metricRepository, IMapper mapper)
    {
        _metricRepository = metricRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<MetricDto>>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _metricRepository.GetPagedMetricsAsync(
            request.ServerId,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<MetricDto>>(items);

        return Result<PagedResult<MetricDto>>.Success(new PagedResult<MetricDto>
        {
            Items = mappedItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
