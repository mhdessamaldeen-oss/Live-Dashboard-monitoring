using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class ReportGenerationJob : IReportGenerationJob, IScopedService
{
    private readonly IReportRepository _reportRepository;
    private readonly IMetricRepository _metricRepository;
    private readonly IReportGenerator _reportGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReportGenerationJob> _logger;
    private readonly string _reportsPath;

    public ReportGenerationJob(
        IReportRepository reportRepository,
        IMetricRepository metricRepository,
        IReportGenerator reportGenerator,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<ReportGenerationJob> logger)
    {
        _reportRepository = reportRepository;
        _metricRepository = metricRepository;
        _reportGenerator = reportGenerator;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
        
        _reportsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "reports");
        if (!Directory.Exists(_reportsPath))
        {
            Directory.CreateDirectory(_reportsPath);
        }
    }

    public async Task GenerateAsync(int reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null) return;

        try
        {
            report.Status = ReportStatus.Processing;
            report.StartedAt = DateTime.UtcNow;
            await _reportRepository.SaveChangesAsync(default);

            var (metrics, _) = await _metricRepository.GetPagedMetricsAsync(
                report.ServerId, 
                report.DateRangeStart, 
                report.DateRangeEnd, 
                1, 5000); 

            var (fileName, contentType, data) = await _reportGenerator.GenerateServerReportAsync(report, metrics);

            var fullPath = Path.Combine(_reportsPath, fileName);
            await File.WriteAllBytesAsync(fullPath, data);

            report.FilePath = fullPath;
            report.FileName = fileName;
            report.ContentType = contentType;
            report.FileSizeBytes = data.Length;
            report.Status = ReportStatus.Completed;
            report.CompletedAt = DateTime.UtcNow;

            await _reportRepository.SaveChangesAsync(default);
            
            await _notificationService.SendReportReadyAsync(report.RequestedByUserId, report.Id, report.Title);

            // Send email notification
            var user = await _userRepository.GetByIdAsync(report.RequestedByUserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Report Ready: {report.Title}",
                    $"Hello {user.FirstName},\n\nYour performance report '{report.Title}' for Server {report.ServerId} is now ready for download.\n\nRegards,\nMonitoring Team"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportId}", reportId);
            report.Status = ReportStatus.Failed;
            report.ErrorMessage = ex.Message;
            await _reportRepository.SaveChangesAsync(default);
        }
    }
}
