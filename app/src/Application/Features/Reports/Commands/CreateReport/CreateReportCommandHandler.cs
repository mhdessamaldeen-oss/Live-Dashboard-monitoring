using Application.Common;
using Application.DTOs.Reports;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
// Hangfire would be used here in a real implementation, but we'll simulate the interface call

namespace Application.Features.Reports.Commands.CreateReport;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Result<ReportDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IServerRepository _serverRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IJobService _jobService;

    public CreateReportCommandHandler(
        IReportRepository reportRepository, 
        IServerRepository serverRepository,
        ICurrentUserService currentUserService, 
        IMapper mapper,
        IJobService jobService)
    {
        _reportRepository = reportRepository;
        _serverRepository = serverRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _jobService = jobService;
    }

    public async Task<Result<ReportDto>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var server = await _serverRepository.GetByIdAsync(request.ServerId, cancellationToken);
        if (server == null)
        {
            return Result<ReportDto>.Failure("Server not found.");
        }

        var report = new Report
        {
            ServerId = request.ServerId,
            Title = request.Title,
            Description = request.Description,
            Status = ReportStatus.Pending,
            DateRangeStart = request.DateRangeStart,
            DateRangeEnd = request.DateRangeEnd,
            RequestedByUserId = _currentUserService.UserId ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        // Enqueue Hangfire job with a continuation for notification
        var jobId = _jobService.Enqueue<IReportGenerationJob>(x => x.GenerateAsync(report.Id));
        
        _jobService.ContinueWith<INotificationService>(jobId, 
            x => x.SendReportReadyAsync(report.RequestedByUserId, report.Id, report.Title, default));

        return Result<ReportDto>.Success(_mapper.Map<ReportDto>(report));
    }
}
