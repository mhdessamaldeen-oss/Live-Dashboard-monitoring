using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a specific permission in the system
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Users.View", "Servers.Provision"
    public string? Description { get; set; }
}
