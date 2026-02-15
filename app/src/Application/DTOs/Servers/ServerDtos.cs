using Domain.Enums;
using Application.DTOs.Metrics;

namespace Application.DTOs.Servers;

public class ServerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? OperatingSystem { get; set; }
    public string Status { get; set; } = ServerStatus.Unknown.ToString();
    public bool IsActive { get; set; }
    public bool IsHost { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class ServerDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? OperatingSystem { get; set; }
    public string Status { get; set; } = ServerStatus.Unknown.ToString();
    public bool IsActive { get; set; }
    public bool IsHost { get; set; }
    public double CpuWarningThreshold { get; set; }
    public double CpuCriticalThreshold { get; set; }
    public double MemoryWarningThreshold { get; set; }
    public double MemoryCriticalThreshold { get; set; }
    public double DiskWarningThreshold { get; set; }
    public double DiskCriticalThreshold { get; set; }
    public List<MetricDto>? LatestMetrics { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateServerRequest(
    string Name,
    string HostName,
    string? IpAddress,
    string? Description,
    string? Location,
    string? OperatingSystem,
    double CpuWarningThreshold = 70.0,
    double CpuCriticalThreshold = 90.0,
    double MemoryWarningThreshold = 70.0,
    double MemoryCriticalThreshold = 90.0,
    double DiskWarningThreshold = 80.0,
    double DiskCriticalThreshold = 95.0);

public record UpdateServerRequest(
    string Name,
    string HostName,
    string? IpAddress,
    string? Description,
    string? Location,
    string? OperatingSystem,
    bool IsActive,
    double CpuWarningThreshold,
    double CpuCriticalThreshold,
    double MemoryWarningThreshold,
    double MemoryCriticalThreshold,
    double DiskWarningThreshold,
    double DiskCriticalThreshold);

public record ServerListQuery(
    string? Search = null,
    ServerStatus? Status = null,
    string? SortBy = "Name",
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 10);
