using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a disk on a monitored server
/// </summary>
public class Disk : BaseEntity
{
    public int ServerId { get; set; }
    public string DriveLetter { get; set; } = string.Empty;
    public long FreeSpaceMB { get; set; }
    public long TotalSpaceMB { get; set; }
    public double UsedPercentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    // public virtual Server? Server { get; set; } // Optional: depends on IF we start using proper navigation props or stick to basic FKs as per hint "without explicit foreign key relationships" (though EF usually likes them).
}
