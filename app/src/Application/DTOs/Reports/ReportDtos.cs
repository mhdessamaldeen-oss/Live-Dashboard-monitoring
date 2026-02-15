using Domain.Enums;

namespace Application.DTOs.Reports;

public record ReportDto
{
    public int Id { get; init; }
    public int ServerId { get; init; }
    public string ServerName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ReportStatus Status { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record CreateReportRequest(
    int ServerId,
    string Title,
    string? Description,
    DateTime? DateRangeStart,
    DateTime? DateRangeEnd);

public record ReportsQuery(
    int? ServerId = null,
    ReportStatus? Status = null,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20);
