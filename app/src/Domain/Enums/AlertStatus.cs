namespace Domain.Enums;

/// <summary>
/// Represents the status of an alert
/// </summary>
public enum AlertStatus
{
    Active = 0,
    Acknowledged = 1,
    Resolved = 2,
    Expired = 3
}
