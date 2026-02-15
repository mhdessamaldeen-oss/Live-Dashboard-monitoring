using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents a system notification triggered when a server metric violates its safety thresholds.
/// Tracks the lifecycle of the issue from triggering to resolution.
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Short summary of the alert (e.g., 'High CPU Usage')
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description including the exact measured value and violation reason
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Current workflow state: Active, Acknowledged, or Resolved
    /// </summary>
    public AlertStatus Status { get; set; } = AlertStatus.Active;

    /// <summary>
    /// Importance level: Warning or Critical
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    /// <summary>
    /// The specific resource type being monitored (CPU, Memory, Disk)
    /// </summary>
    public string? MetricType { get; set; }

    /// <summary>
    /// The measured value that triggered the alert
    /// </summary>
    public double? MetricValue { get; set; }

    /// <summary>
    /// The threshold limit that was exceeded
    /// </summary>
    public double? ThresholdValue { get; set; }

    /// <summary>
    /// Timestamp when an operator manually acknowledged the alert
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// ID of the user who acknowledged the alert
    /// </summary>
    public int? AcknowledgedByUserId { get; set; }

    /// <summary>
    /// Timestamp when the issue was marked as resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// ID of the user who resolved the alert
    /// </summary>
    public int? ResolvedByUserId { get; set; }

    /// <summary>
    /// Notes describing how the issue was fixed or addressed
    /// </summary>
    public string? Resolution { get; set; }
    
    // Foreign key
    public int ServerId { get; set; }
    
    /// <summary>
    /// Anti-spam: Check if enough time has passed since the last similar alert
    /// </summary>
    public static bool CanCreateAlert(DateTime? lastAlertTime, TimeSpan antiSpamWindow)
    {
        if (!lastAlertTime.HasValue) return true;
        return DateTime.UtcNow - lastAlertTime.Value >= antiSpamWindow;
    }
}
