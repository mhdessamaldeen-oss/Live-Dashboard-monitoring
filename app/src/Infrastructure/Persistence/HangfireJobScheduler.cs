using Application.Interfaces;
using Hangfire;
using Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class HangfireJobScheduler
{
    public static void ScheduleJobs(IServiceProvider serviceProvider)
    {
        var jobService = serviceProvider.GetRequiredService<IJobService>();
        
        jobService.AddOrUpdateRecurringJob<MetricCollectionJob>(
            "metric-collection",
            x => x.RunAsync(),
            "*/30 * * * * *");

        jobService.AddOrUpdateRecurringJob<AlertCleanupJob>(
            "alert-cleanup",
            x => x.RunAsync(),
            Cron.Hourly());

        jobService.AddOrUpdateRecurringJob<SystemHealthCheckJob>(
            "system-health-check",
            x => x.RunAsync(),
            Cron.Daily());

        jobService.AddOrUpdateRecurringJob<ReportMaintenanceJob>(
            "report-automated-dispatch",
            x => x.RunAsync(),
            "*/5 * * * *");
    }
}
