namespace Application.DTOs.Metrics;

public class DiskDto
{
    public string DriveLetter { get; set; } = string.Empty;
    public long FreeSpaceMB { get; set; }
    public long TotalSpaceMB { get; set; }
    public double UsedPercentage { get; set; }
}
