using Application.Common;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<Result<IEnumerable<UserDto>>>;
