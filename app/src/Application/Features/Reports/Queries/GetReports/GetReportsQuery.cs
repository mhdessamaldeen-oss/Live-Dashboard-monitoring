using Application.Common;
using Application.DTOs.Reports;
using Domain.Enums;
using MediatR;

namespace Application.Features.Reports.Queries.GetReports;

public record GetReportsQuery(
    int? ServerId = null,
    ReportStatus? Status = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ReportDto>>>;
