using Application.Common;
using Application.DTOs.Alerts;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Alerts.Commands.ResolveAlert;

public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, Result<AlertDto>>
{
    private readonly IAlertRepository _alertRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public ResolveAlertCommandHandler(
        IAlertRepository alertRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _alertRepository = alertRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Result<AlertDto>> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetWithServerByIdAsync(request.Id, cancellationToken);

        if (alert == null)
        {
            return Result<AlertDto>.Failure("Alert not found.");
        }

        if (alert.Status == AlertStatus.Resolved)
        {
            return Result<AlertDto>.Failure("Alert is already resolved.");
        }

        alert.Status = AlertStatus.Resolved;
        alert.Resolution = request.Resolution;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedByUserId = _currentUserService.UserId;

        await _alertRepository.SaveChangesAsync(cancellationToken);
        
        var alertDto = _mapper.Map<AlertDto>(alert);
        await _notificationService.SendAlertResolvedAsync(alertDto, cancellationToken);

        return Result<AlertDto>.Success(alertDto);
    }
}
