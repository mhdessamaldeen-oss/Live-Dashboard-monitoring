using Domain.Entities;
using Infrastructure.Services;
using System.Text;
using Xunit;

namespace Application.Tests.Services;

public class CsvReportGeneratorTests
{
    [Fact]
    public async Task GenerateServerReportAsync_ShouldReturnCsvData()
    {
        // Arrange
        var generator = new CsvReportGenerator();
        var report = new Report
        {
            Title = "Test Performance Report",
            ServerId = 1,
            DateRangeStart = DateTime.UtcNow.AddDays(-1),
            DateRangeEnd = DateTime.UtcNow
        };

        var metrics = new List<Metric>
        {
            new Metric { Timestamp = DateTime.UtcNow, CpuUsagePercent = 45.5, MemoryUsagePercent = 60.0 },
            new Metric { Timestamp = DateTime.UtcNow.AddHours(-1), CpuUsagePercent = 30.2, MemoryUsagePercent = 55.0 }
        };

        // Act
        var (fileName, contentType, data) = await generator.GenerateServerReportAsync(report, metrics);

        // Assert
        Assert.EndsWith(".csv", fileName);
        Assert.Equal("text/csv", contentType);
        Assert.NotEmpty(data);

        var csvString = Encoding.UTF8.GetString(data);
        Assert.Contains("Report Title,Test Performance Report", csvString);
        Assert.Contains("45.50", csvString);
        Assert.Contains("Timestamp,CPU Usage %", csvString);
    }
}
