using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Alerts.Commands.AcknowledgeAlert;

public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, Result>
{
    private readonly IAlertRepository _alertRepository;

    public AcknowledgeAlertCommandHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<Result> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(request.Id, cancellationToken);

        if (alert == null)
        {
            return Result.Failure("Alert not found");
        }

        if (alert.Status == AlertStatus.Resolved)
        {
            return Result.Failure("Cannot acknowledge a resolved alert");
        }

        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedAt = DateTime.UtcNow;

        await _alertRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
