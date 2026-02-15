using Application.DTOs.Jobs;
using System.Linq.Expressions;

namespace Application.Interfaces;

public interface IJobService
{
    string Enqueue(Expression<Func<Task>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    
    void ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall);
    
    void AddOrUpdateRecurringJob(string jobName, Expression<Func<Task>> methodCall, string cronExpression);
    void AddOrUpdateRecurringJob<T>(string jobName, Expression<Func<T, Task>> methodCall, string cronExpression);
    
    Task<List<JobStatusDto>> GetRecurringJobsAsync();
}
