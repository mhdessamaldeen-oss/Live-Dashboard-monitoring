namespace Domain.Enums;

/// <summary>
/// Represents the status of a report generation job
/// </summary>
public enum ReportStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
