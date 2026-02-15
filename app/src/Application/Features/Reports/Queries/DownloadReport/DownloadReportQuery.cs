using Application.Common;
using MediatR;

namespace Application.Features.Reports.Queries.DownloadReport;

public record DownloadReportQuery(int Id) : IRequest<Result<ReportFileDto>>;

public record ReportFileDto(
    byte[] Content,
    string FileName,
    string ContentType);
