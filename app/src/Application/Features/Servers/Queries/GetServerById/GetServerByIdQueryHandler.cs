using Application.Common;
using Application.DTOs.Metrics;
using Application.DTOs.Servers;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Servers.Queries.GetServerById;

public class GetServerByIdQueryHandler : IRequestHandler<GetServerByIdQuery, Result<ServerDetailDto>>
{
    private readonly IServerRepository _serverRepository;
    private readonly IMapper _mapper;

    public GetServerByIdQueryHandler(IServerRepository serverRepository, IMapper mapper)
    {
        _serverRepository = serverRepository;
        _mapper = mapper;
    }

    public async Task<Result<ServerDetailDto>> Handle(GetServerByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _serverRepository.GetServerDetailsAsync(request.Id, cancellationToken);
        
        if (result == null)
        {
            return Result<ServerDetailDto>.Failure("Server not found.");
        }

        var (server, metrics) = result.Value;

        var dto = _mapper.Map<ServerDetailDto>(server);
        
        // Populate collections manually since navigation properties are gone
        var metricDtos = _mapper.Map<List<MetricDto>>(metrics);
        
        dto.LatestMetrics = metricDtos;

        return Result<ServerDetailDto>.Success(dto);
    }
}
