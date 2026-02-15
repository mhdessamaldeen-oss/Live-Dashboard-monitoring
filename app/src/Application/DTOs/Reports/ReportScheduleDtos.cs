namespace Application.DTOs.Reports;

public record ReportScheduleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CronExpression { get; init; } = string.Empty;
    public DateTime? LastRunAt { get; init; }
    public DateTime? NextRunAt { get; init; }
    public bool IsActive { get; init; }
    public string Recipients { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public int ServerId { get; init; }
    public string? ServerName { get; init; }
}

public record CreateReportScheduleRequest(
    string Name,
    string? Description,
    string CronExpression,
    string Recipients,
    string ReportType,
    int ServerId);

public record UpdateReportScheduleRequest(
    string Name,
    string? Description,
    string CronExpression,
    string Recipients,
    string ReportType,
    int ServerId,
    bool IsActive);
