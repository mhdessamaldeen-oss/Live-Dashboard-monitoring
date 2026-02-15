using Application.Interfaces;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using Application.DTOs.Jobs;

namespace Infrastructure.Services;

public class JobService : IJobService, IScopedService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IReportScheduleRepository _reportScheduleRepository;

    public JobService(
        IBackgroundJobClient backgroundJobClient, 
        IRecurringJobManager recurringJobManager,
        IReportScheduleRepository reportScheduleRepository)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _reportScheduleRepository = reportScheduleRepository;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue<T>(methodCall);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClient.Schedule<T>(methodCall, delay);
    }

    public void ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        _backgroundJobClient.ContinueJobWith<T>(parentJobId, methodCall);
    }

    public void AddOrUpdateRecurringJob(string jobName, Expression<Func<Task>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(jobName, methodCall, cronExpression);
    }

    public void AddOrUpdateRecurringJob<T>(string jobName, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate<T>(jobName, methodCall, cronExpression);
    }

    public async Task<List<JobStatusDto>> GetRecurringJobsAsync()
    {
        // 1. Fetch system jobs from Hangfire
        using var connection = JobStorage.Current.GetConnection();
        var hangfireJobs = connection.GetRecurringJobs();
        
        var jobs = hangfireJobs.Select(j => new JobStatusDto(
            j.Id,
            j.Id, 
            j.LastJobState ?? "Scheduled",
            j.LastExecution,
            j.NextExecution,
            j.Cron
        )).ToList();

        // 2. Fetch user-defined report schedules from the database
        var userSchedules = await _reportScheduleRepository.GetAllWithServerNamesAsync();
        
        foreach (var schedule in userSchedules)
        {
            jobs.Add(new JobStatusDto(
                $"report-schedule-{schedule.Id}",
                $"[Report] {schedule.Name}",
                schedule.IsActive ? "Active" : "Paused",
                schedule.LastRunAt,
                schedule.NextRunAt,
                schedule.CronExpression
            ));
        }

        return jobs;
    }
}
