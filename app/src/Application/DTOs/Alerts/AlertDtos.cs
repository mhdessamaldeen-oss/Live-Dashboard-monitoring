using Domain.Enums;

namespace Application.DTOs.Alerts;

public class AlertDto
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? MetricType { get; set; }
    public double? MetricValue { get; set; }
    public double? ThresholdValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public record AlertsQuery(
    int? ServerId = null,
    AlertStatus? Status = null,
    AlertSeverity? Severity = null,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20);

public record ResolveAlertRequest(string? Resolution);

public record AlertSummaryDto(
    int TotalAlerts,
    int ActiveAlerts,
    int CriticalAlerts,
    int WarningAlerts,
    int InfoAlerts);
