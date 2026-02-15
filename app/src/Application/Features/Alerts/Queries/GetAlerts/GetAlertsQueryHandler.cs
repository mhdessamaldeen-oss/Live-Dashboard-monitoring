using Application.Common;
using Application.DTOs.Alerts;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Alerts.Queries.GetAlerts;

public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, Result<PagedResult<AlertDto>>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IMapper _mapper;

    public GetAlertsQueryHandler(IAlertRepository alertRepository, IMapper mapper)
    {
        _alertRepository = alertRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AlertDto>>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var result = await _alertRepository.GetPagedAlertsAsync(
            request.ServerId,
            request.Status,
            request.Severity,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = result.Items;
        var totalCount = result.TotalCount;

        var mappedItems = new List<AlertDto>();
        if (items != null)
        {
            foreach (var row in items)
            {
                mappedItems.Add(new AlertDto
                {
                    Id = row.Id,
                    ServerId = row.ServerId,
                    ServerName = row.ServerName,
                    Title = row.Title,
                    Message = row.Message,
                    Status = ((Domain.Enums.AlertStatus)row.Status).ToString(),
                    Severity = ((Domain.Enums.AlertSeverity)row.Severity).ToString(),
                    MetricType = row.MetricType,
                    MetricValue = row.MetricValue,
                    ThresholdValue = row.ThresholdValue,
                    CreatedAt = row.CreatedAt,
                    ResolvedAt = row.ResolvedAt
                });
            }
        }

        return Result<PagedResult<AlertDto>>.Success(new PagedResult<AlertDto>
        {
            Items = mappedItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
