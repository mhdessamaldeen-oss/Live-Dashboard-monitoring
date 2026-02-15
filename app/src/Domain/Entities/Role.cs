using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a user role in the system
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
