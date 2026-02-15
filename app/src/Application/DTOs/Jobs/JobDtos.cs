namespace Application.DTOs.Jobs;

public record JobStatusDto(
    string Id,
    string Name,
    string Status,
    DateTime? LastRun,
    DateTime? NextRun,
    string CronExpression);
