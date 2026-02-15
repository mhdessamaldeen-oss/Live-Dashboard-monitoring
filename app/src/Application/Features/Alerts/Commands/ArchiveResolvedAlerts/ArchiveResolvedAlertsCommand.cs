using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Alerts.Commands.ArchiveResolvedAlerts;

public record ArchiveResolvedAlertsCommand : IRequest<Result<int>>;

public class ArchiveResolvedAlertsCommandHandler : IRequestHandler<ArchiveResolvedAlertsCommand, Result<int>>
{
    private readonly IAlertRepository _alertRepository;

    public ArchiveResolvedAlertsCommandHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<Result<int>> Handle(ArchiveResolvedAlertsCommand request, CancellationToken cancellationToken)
    {
        var count = await _alertRepository.ArchiveResolvedAlertsAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
