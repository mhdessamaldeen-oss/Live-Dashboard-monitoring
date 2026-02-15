using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace Infrastructure.BackgroundJobs;

public class ReportMaintenanceJob : IScopedService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportScheduleRepository _scheduleRepository;
    private readonly IJobService _jobService;
    private readonly ILogger<ReportMaintenanceJob> _logger;

    public ReportMaintenanceJob(
        IReportRepository reportRepository, 
        IReportScheduleRepository scheduleRepository,
        IJobService jobService,
        ILogger<ReportMaintenanceJob> logger)
    {
        _reportRepository = reportRepository;
        _scheduleRepository = scheduleRepository;
        _jobService = jobService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting report maintenance and automated dispatch check...");

        // 1. Check for automated schedules that are due
        var now = DateTime.UtcNow;
        var dueSchedules = await _scheduleRepository.FindAsync(s => s.IsActive && s.NextRunAt <= now);

        foreach (var schedule in dueSchedules)
        {
            try
            {
                _logger.LogInformation("Processing scheduled report: {Name} for Server {ServerId}", schedule.Name, schedule.ServerId);

                // Create the report record
                var report = new Report
                {
                    Title = $"{schedule.Name} (Auto-generated)",
                    Description = schedule.Description,
                    Status = ReportStatus.Pending,
                    ServerId = schedule.ServerId,
                    RequestedByUserId = schedule.CreatedByUserId,
                    CreatedAt = now,
                    DateRangeEnd = now,
                    DateRangeStart = schedule.LastRunAt ?? now.AddDays(-1)
                };

                await _reportRepository.AddAsync(report);
                await _reportRepository.SaveChangesAsync();

                // Enqueue the actual generation job
                _jobService.Enqueue<IReportGenerationJob>(x => x.GenerateAsync(report.Id));

                // Update schedule metadata
                schedule.LastRunAt = now;
                var crontab = CrontabSchedule.Parse(schedule.CronExpression);
                schedule.NextRunAt = crontab.GetNextOccurrence(now);
                
                _scheduleRepository.Update(schedule);
                _logger.LogInformation("Scheduled report {Name} enqueued. Next run: {NextRun}", schedule.Name, schedule.NextRunAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled report {ScheduleId}", schedule.Id);
            }
        }

        await _scheduleRepository.SaveChangesAsync();
        
        // 2. Clean up old report files (older than 30 days) - Placeholder for now
        _logger.LogInformation("Report maintenance completed successfully.");
    }
}
