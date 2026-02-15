using Domain.Entities;
using Application.DTOs.Reports;

namespace Application.Interfaces;

public interface IReportScheduleRepository : IRepository<ReportSchedule>
{
    Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportScheduleDto>> GetAllWithServerNamesAsync(CancellationToken cancellationToken = default);
}
