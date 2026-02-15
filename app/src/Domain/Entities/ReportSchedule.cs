using Domain.Common;

namespace Domain.Entities;

public class ReportSchedule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string Recipients { get; set; } = string.Empty; // Comma separated emails
    public string ReportType { get; set; } = "Summary"; // Summary, Performance, Security, etc.

    // Configuration for the report
    public int ServerId { get; set; }
    public int CreatedByUserId { get; set; }
}
