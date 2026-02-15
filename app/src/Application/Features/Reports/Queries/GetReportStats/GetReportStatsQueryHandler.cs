using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Reports.Queries.GetReportStats;

public record ReportStatsDto(
    int TotalReports,
    int ScheduledReports,
    int DownloadsToday,
    int SuccessRate);

public record GetReportStatsQuery : IRequest<Result<ReportStatsDto>>;

public class GetReportStatsQueryHandler : IRequestHandler<GetReportStatsQuery, Result<ReportStatsDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportScheduleRepository _scheduleRepository;

    public GetReportStatsQueryHandler(IReportRepository reportRepository, IReportScheduleRepository scheduleRepository)
    {
        _reportRepository = reportRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result<ReportStatsDto>> Handle(GetReportStatsQuery request, CancellationToken cancellationToken)
    {
        var total = await _reportRepository.CountAsync(cancellationToken);
        var scheduled = await _scheduleRepository.CountAsync(s => s.IsActive, cancellationToken);
        
        // Success rate calculation
        var completedCount = await _reportRepository.CountAsync(r => r.Status == ReportStatus.Completed, cancellationToken);
        var failedCount = await _reportRepository.CountAsync(r => r.Status == ReportStatus.Failed, cancellationToken);
        
        int successRate = 100;
        if (completedCount + failedCount > 0)
        {
            successRate = (completedCount * 100) / (completedCount + failedCount);
        }

        // Downloads today (simulated for now based on completed reports today)
        var today = DateTime.UtcNow.Date;
        var createdToday = await _reportRepository.CountAsync(r => r.CreatedAt >= today, cancellationToken);

        return Result<ReportStatsDto>.Success(new ReportStatsDto(
            total,
            scheduled,
            createdToday, // Rough estimate of activity today
            successRate
        ));
    }
}
