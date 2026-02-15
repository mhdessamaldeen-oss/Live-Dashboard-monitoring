using Application.Common;
using Application.DTOs.Servers;
using Domain.Enums;
using MediatR;

namespace Application.Features.Servers.Queries.GetServers;

public record GetServersQuery(
    string? Search = null,
    ServerStatus? Status = null,
    string? SortBy = "Id",
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ServerDto>>>;
