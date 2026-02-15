namespace Domain.Enums;

/// <summary>
/// Represents the operational status of a server
/// </summary>
public enum ServerStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Warning = 3,
    Critical = 4,
    Maintenance = 5
}
