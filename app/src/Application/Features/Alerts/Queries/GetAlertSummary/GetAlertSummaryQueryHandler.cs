using Application.Common;
using Application.DTOs.Alerts;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Alerts.Queries.GetAlertSummary;

public class GetAlertSummaryQueryHandler : IRequestHandler<GetAlertSummaryQuery, Result<AlertSummaryDto>>
{
    private readonly IAlertRepository _alertRepository;

    public GetAlertSummaryQueryHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<Result<AlertSummaryDto>> Handle(GetAlertSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _alertRepository.GetAlertSummaryAsync(cancellationToken);
        return Result<AlertSummaryDto>.Success(summary);
    }
}
