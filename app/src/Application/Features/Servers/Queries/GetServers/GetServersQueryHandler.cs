using Application.Common;
using Application.DTOs.Servers;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Servers.Queries.GetServers;

public class GetServersQueryHandler : IRequestHandler<GetServersQuery, Result<PagedResult<ServerDto>>>
{
    private readonly IServerRepository _serverRepository;
    private readonly IMetricsProviderSettings _settings;
    private readonly IMapper _mapper;

    public GetServersQueryHandler(IServerRepository serverRepository, IMetricsProviderSettings settings, IMapper mapper)
    {
        _serverRepository = serverRepository;
        _settings = settings;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ServerDto>>> Handle(GetServersQuery request, CancellationToken cancellationToken)
    {
        bool onlyHost = _settings.CurrentMode == MetricsProviderMode.System;

        var (items, totalCount) = await _serverRepository.GetPagedServersAsync(
            request.Search,
            request.Status,
            request.SortBy ?? "Id",
            request.SortDescending,
            request.Page,
            request.PageSize,
            onlyHost,
            cancellationToken);

        var mappedItems = _mapper.Map<List<ServerDto>>(items);

        return Result<PagedResult<ServerDto>>.Success(new PagedResult<ServerDto>
        {
            Items = mappedItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
