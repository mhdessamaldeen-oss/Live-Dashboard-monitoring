using Application.Common;
using Application.DTOs.Servers;
using MediatR;

namespace Application.Features.Servers.Queries.GetServerById;

public record GetServerByIdQuery(int Id) : IRequest<Result<ServerDetailDto>>;
