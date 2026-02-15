using Domain.Entities;

namespace Application.Interfaces;

public interface IReportGenerator
{
    Task<(string FileName, string ContentType, byte[] Data)> GenerateServerReportAsync(Report report, IEnumerable<Metric> metrics, CancellationToken cancellationToken = default);
}
