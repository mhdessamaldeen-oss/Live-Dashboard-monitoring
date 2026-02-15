using Application.Common;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role = "User") : IRequest<Result<UserDto>>;
