using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries.DownloadReport;

public class DownloadReportQueryHandler : IRequestHandler<DownloadReportQuery, Result<ReportFileDto>>
{
    private readonly IApplicationDbContext _context;

    public DownloadReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReportFileDto>> Handle(DownloadReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _context.Reports.FindAsync(request.Id);

        if (report == null)
        {
            return Result<ReportFileDto>.Failure("Report not found.");
        }

        if (report.Status != Domain.Enums.ReportStatus.Completed || string.IsNullOrEmpty(report.FilePath))
        {
            return Result<ReportFileDto>.Failure("Report is not ready or failed generation.");
        }

        if (!File.Exists(report.FilePath))
        {
             return Result<ReportFileDto>.Failure("Report file missing from storage.");
        }

        var content = await File.ReadAllBytesAsync(report.FilePath, cancellationToken);
        
        return Result<ReportFileDto>.Success(new ReportFileDto(
            content,
            report.FileName ?? "report.pdf",
            report.ContentType ?? "application/pdf"
        ));
    }
}
