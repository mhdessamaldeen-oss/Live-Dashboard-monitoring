using Application.Interfaces;
using Domain.Entities;
using System.Text;

namespace Infrastructure.Services;

public class CsvReportGenerator : IReportGenerator, IScopedService
{
    public Task<(string FileName, string ContentType, byte[] Data)> GenerateServerReportAsync(
        Report report, 
        IEnumerable<Metric> metrics, 
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Report Title," + report.Title);
        sb.AppendLine("Server ID," + report.ServerId);
        sb.AppendLine("Date Range," + (report.DateRangeStart?.ToString("yyyy-MM-dd") ?? "Start") + " to " + (report.DateRangeEnd?.ToString("yyyy-MM-dd") ?? "End"));
        sb.AppendLine("Generated At," + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        sb.AppendLine();
        
        // Column Headers
        sb.AppendLine("Timestamp,CPU Usage %,Memory Usage %,Network In (B/s),Network Out (B/s),Status");
        
        // Data Rows
        foreach (var m in metrics.OrderByDescending(x => x.Timestamp))
        {
            sb.AppendLine($"{m.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                          $"{m.CpuUsagePercent:F2}," +
                          $"{m.MemoryUsagePercent:F2}," +
                          $"{m.NetworkInBytesPerSec:F2}," +
                          $"{m.NetworkOutBytesPerSec:F2}," +
                          $"Online");
        }
        
        var fileName = $"Report_{report.ServerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var data = Encoding.UTF8.GetBytes(sb.ToString());
        
        return Task.FromResult((fileName, "text/csv", data));
    }
}
