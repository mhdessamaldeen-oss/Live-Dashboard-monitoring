using Application.Common;
using Application.DTOs.Reports;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Reports.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<PagedResult<ReportDto>>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IMapper _mapper;

    public GetReportsQueryHandler(IReportRepository reportRepository, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ReportDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var result = await _reportRepository.GetPagedReportsAsync(
            request.ServerId,
            request.Status,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = result.Items;
        var totalCount = result.TotalCount;

        var mappedItems = new List<ReportDto>();
        foreach (var row in items)
        {
            mappedItems.Add(new ReportDto
            {
                Id = row.Id,
                ServerId = row.ServerId,
                ServerName = row.ServerName ?? "Unknown Node",
                Title = row.Title,
                Description = row.Description,
                Status = (Domain.Enums.ReportStatus)row.Status,
                FileName = row.FileName,
                FileSizeBytes = row.FileSizeBytes,
                CreatedAt = row.CreatedAt,
                CompletedAt = row.CompletedAt
            });
        }

        return Result<PagedResult<ReportDto>>.Success(new PagedResult<ReportDto>
        {
            Items = mappedItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
